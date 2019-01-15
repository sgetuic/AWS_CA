using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.IoT;
using Amazon.IoT.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Function
{
    class Function
    {
        /*Private vars*/
        private string _CRTid, _serialNr, _name;
        private long _timeStampUTC, _registrationTimeStampUTC;
        private Dictionary<string, AttributeValue> _thingTableItem;

        /* DynamoDB key names.*/
        private readonly string TableName = "DeviceSerialNumbers";
        private readonly string SerialNrKey = "SerialNr";
        private readonly string RegistredBoolKey = "Registered";
        private readonly string TimeStampUTCKey = "TimeStampUTC";
        private readonly string RegistrationTimeStampCKey = "TimeStampRegisteredUTC";
        private readonly string RegistrationSuccessfullKey = "RegistrationSuccessfull";
        private readonly string CommentKey = "RegistrationComment";

        /*Policy name to be attached to the Thing in the AWS IoT Core*/
        private readonly string PolicyName = "OnboardedDevice";
        /*Group Names for moving the Thing accordingly.*/
        private readonly string OnboardingFailedGroupName = "OnboardingFailed";
        private readonly string OnboardedGroupName = "OnboardedDevices";

        /*Registration Failure Reasons*/

        private readonly string FailedString_CouldNotRetrieveFromDBString = "Device info not found in DB";
        private readonly string FailedString_TimeStampInvalid = "TimeStamp did not validate";
        private readonly string FailedString_PolicyAttaching = "Failed to Attach the Policy";
        private readonly string FailedString_CertificateAttaching = "Failed to Attach the Certificate";
        private readonly string FailedString_UpdateDB = "Failed to update Database";
        private readonly string FailedString_AlreadyRegistered = "SerialNr was already redistered";
        private readonly string SuccessString = "OK";


        /*Amazon service clients*/
        IAmazonIoT _IoTclient = new AmazonIoTClient();
        AmazonDynamoDBClient _dbClient = new AmazonDynamoDBClient();

        /*Time Frame for TimeStamp Validation in Minutes.*/
        private readonly int ExpirationTimeFrameMin = 10;




        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<bool> FunctionHandler(JObject input, ILambdaContext context)
        {

            //TODO: Make sure these are the same as in the SerialNumberRegistrator dispatched event.
            _CRTid = (string)input["attributes"]["CRTId"];
            _serialNr = (string)input["attributes"]["SerialNr"];
            _name = (string)input["thingName"];

            _registrationTimeStampUTC = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            bool registrationDone = false;

            if (await getThingItemFromDB())
            {
                if (isRegistrationValid())
                {
                   
                    if (await attachPolicyToCertificateAsync())
                    {
                        if (await attachCertificateToThingAsync())
                        {
                            if (await upDateItem(TableName, _serialNr, true, _timeStampUTC, _registrationTimeStampUTC, true, SuccessString))
                            {
                                await moveThingToGroup(OnboardedGroupName);
                                registrationDone = true;
                            }
                            else
                            {
                                LambdaLogger.Log("upDateItemFailed");
                                registrationFailed(FailedString_UpdateDB);
                            }
                        }
                        else
                        {
                            LambdaLogger.Log("certifaceAttaching failed");
                            registrationFailed(FailedString_CertificateAttaching);
                        }
                    }
                    else
                    {
                        LambdaLogger.Log("policyAttached failed");
                        registrationFailed(FailedString_PolicyAttaching);
                    }
                }
                else
                {
                    LambdaLogger.Log("isRegistrationValid() failed");
                }

            }
            else {
                LambdaLogger.Log("getThingItemFromDB() failed");
                registrationFailed(FailedString_CouldNotRetrieveFromDBString);
            }

         
            return registrationDone;
        }

        private async void registrationFailed(string reason)
        {
            await upDateItem(TableName, _serialNr, false, _timeStampUTC, _registrationTimeStampUTC, false, reason);
            await moveThingToGroup(OnboardingFailedGroupName);
        }

        /**Moves the Thing to a named Group*/
        private async Task<bool> moveThingToGroup(string groupName)
        {
            var request = new AddThingToThingGroupRequest();
            request.ThingName = _name;
            request.ThingGroupName = groupName;
            AddThingToThingGroupResponse response = await _IoTclient.AddThingToThingGroupAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        /**Gets the Database Table Item for the Device from the Registration Event.
         * 
         * https://docs.aws.amazon.com/sdkfornet1/latest/apidocs/html/M_Amazon_DynamoDB_AmazonDynamoDB_GetItem.htm
         * https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.SDKs.Interfaces.LowLevel.html
         */
        private async Task<bool> getThingItemFromDB()
        {
            // Create GetItem request
            var value = new AttributeValue();

            value.S = _serialNr;
            var key = new Dictionary<string, AttributeValue>();
            key.Add(SerialNrKey, value);

            var att = new List<string>();
            att.Add(RegistredBoolKey);
            att.Add(TimeStampUTCKey);

            GetItemRequest request = new GetItemRequest
            {
                TableName = TableName,
                Key = key,
                AttributesToGet = att,
                ConsistentRead = true
            };


            // Issue request
            var result = await _dbClient.GetItemAsync(request);
            LambdaLogger.Log("\ngetThingItemFromDB(): "+result.ToString());
            LambdaLogger.Log("\nresult.Item.Count: "+ result.Item.Count);
            if (result.Item.Count > 0)
            {
                _thingTableItem = result.Item;
               
                return true;
            }
            return false;

        }
        /**Checks if the Table Item for the Device to be Registered in AWS IoT core is valid*/
        private bool isRegistrationValid() {
            LambdaLogger.Log(_thingTableItem.ToString());

            if (isRegistered()) {
                LambdaLogger.Log("Already Registered");
                registrationFailed(FailedString_AlreadyRegistered);

                return false;
            }

            if (!isTimeStampValid())
            {
                LambdaLogger.Log("Time Stamp Expired");
                registrationFailed(FailedString_TimeStampInvalid);
                return false;
            }
            LambdaLogger.Log("RegistrationValid");
            return true;
        }


        /**Checks if TimeStamp for the Device is still valid*/
        private bool isTimeStampValid()
        {
            var val = new AttributeValue();

            _thingTableItem.TryGetValue(TimeStampUTCKey, out val);


            _timeStampUTC = Convert.ToInt64(val.N);
            LambdaLogger.Log("\n timestamp>"+ _timeStampUTC.ToString());
            LambdaLogger.Log("\n timestamp -current time = Min:"+ (_registrationTimeStampUTC - _timeStampUTC));


            return (_registrationTimeStampUTC - _timeStampUTC < ExpirationTimeFrameMin * 60000);
        }

        /**Checks if Table Item for the Device to be registered is not registered yet. */
        private bool isRegistered()
        {
            var val = new AttributeValue();

            _thingTableItem.TryGetValue(RegistredBoolKey, out val);
            LambdaLogger.Log(val.BOOL.ToString());

            return val.BOOL;
        }

        /**Attaches the Policy to the Thing in AWS IoT Core*/
        private async Task<bool> attachPolicyToCertificateAsync()
        {
            var request = new AttachPolicyRequest();
            request.Target = _CRTid;
            request.PolicyName = PolicyName;
            AttachPolicyResponse response = await _IoTclient.AttachPolicyAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        /**Attaches the Certificate to the Thing in AWS IoT Core*/
        private async Task<bool> attachCertificateToThingAsync()
        {
            var request = new AttachThingPrincipalRequest();
            request.Principal = _CRTid;
            request.ThingName = _name;
            AttachThingPrincipalResponse response = await _IoTclient.AttachThingPrincipalAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }




        /**Updates Table Item for the registrated Device in the Database.
         */
        private async Task<bool> upDateItem(string tableName, string serialNr, bool registered, long timeStampUTC, long registrationTimeStampUTC, bool registrationSuccessful, string comment)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    { SerialNrKey, new AttributeValue {
                        S = serialNr
                    }},
                    { RegistredBoolKey, new AttributeValue {
                        BOOL = true
                    }},
                    { TimeStampUTCKey, new AttributeValue {
                        N = timeStampUTC.ToString()
                    }},
                    { RegistrationTimeStampCKey, new AttributeValue {
                        N = registrationTimeStampUTC.ToString()
                    }},
                    { RegistrationSuccessfullKey, new AttributeValue
                    {
                        BOOL = registrationSuccessful
                    }},
                     { CommentKey, new AttributeValue
                    {
                        S = comment
                    }}

                },
                ConditionExpression = "attribute_exists(Registered)"

            };

            PutItemResponse response = await _dbClient.PutItemAsync(request);
            return response.HttpStatusCode== System.Net.HttpStatusCode.OK;
        }

    }
}

