using System;
using System.IO;
using System.Net;
using Amazon;
using Amazon.IoT;
using Amazon.IoT.Model;

/* 
 */
namespace UIC.AWS.Onboarding.Device
{
    internal class Onboard
    {
        private static readonly        IAmazonIoT IoT               = new AmazonIoTClient();

        private static readonly string DeviceCertAndKeyLocation     = AWSConfigs.GetConfig("DeviceCertAndKeySaveLocation");

        private static readonly string CredentialsFileLocation      = AWSConfigs.AWSProfilesLocation;

        private static readonly string DeviceCertFileLocation       = DeviceCertAndKeyLocation +  AWSConfigs.GetConfig("DeviceCrtFileName");

        private static readonly string DevicePrivateKeyFileLocation = DeviceCertAndKeyLocation + AWSConfigs.GetConfig("DeviceKeyFileName");

        private static readonly string SerialNr                     = AWSConfigs.GetConfig("DeviceSerialNumber");

        /**Device name. Will be concatenated with SerialNr */
        private static readonly string DeviceName                   = AWSConfigs.GetConfig("DeviceName")+ SerialNr; 

        private static string _CRTid;


        public static void Main(string[] args)
        {
            bool success = false;
            if (!checkIfCrtOrKeyExist())
            {
                Console.WriteLine("Starting Onboarding");

                success = CreateCertAndKeys();

                success = CreateThing();


                if (DeleteCredentials())
                {
                    Console.WriteLine("Cerdentials Deleted not safely");
                }
                else
                {
                    Console.WriteLine("Credentials not Deleted somehow...");
                }


                Console.WriteLine("Onboarding finished successfully: "+success);
                return;
            }
            else
            {
                Console.WriteLine("There already Exists a key or crt file! Onboarding not started.");
                return;
            }
        }


       /** Creates Certificates and Keys in IoT Core, downloads them and writes them to files.
        * This particular Crt and key combination can be downloaded only once from the IoT Core.
       */
        private static bool CreateCertAndKeys()
        {

            CreateKeysAndCertificateResponse response = getKeyAndCertResponse();
            _CRTid = response.CertificateArn;

            return WriteKeyCertToFile(response.KeyPair.PrivateKey, response.CertificatePem);
                        
        }


        /**Askes AWS IoT for creation and sending of Certificate and Key.
         */
        private static CreateKeysAndCertificateResponse getKeyAndCertResponse()
        {
            CreateKeysAndCertificateRequest request = new CreateKeysAndCertificateRequest { SetAsActive = true };
            return IoT.CreateKeysAndCertificate(request);
        }


        /** Creates a thing in IoT Core register.
         *  Serial Number And CRT id will be added as Thing Attributes.
         */
        private static bool CreateThing()
        {
      
            var response = IoT.CreateThing(
                makeThingRequest(
                    makeAttributePayloadForThingCreation()));

            Console.WriteLine($"thing creation response: {response.ThingArn}");

            return(response.HttpStatusCode == HttpStatusCode.OK);
        }

        /**Creates an AttributePayoad with the necessary Attributes for Thing creation in AWS IoT.
         * 
         */ 
        private static AttributePayload makeAttributePayloadForThingCreation()
        {

            var attributePayload = new AttributePayload();
            attributePayload.Attributes.Add("SerialNr", SerialNr);
            attributePayload.Attributes.Add("CRTId", _CRTid);

            return attributePayload;
        }

        /**Creates an AttributePayoad with the necessary Attributes for Thing creation in AWS IoT.
        * 
        */
        private static CreateThingRequest makeThingRequest(AttributePayload attributePayload)
        {
            CreateThingRequest thingRequest = new CreateThingRequest();
            thingRequest.ThingName = DeviceName;
            thingRequest.AttributePayload = attributePayload;

            return thingRequest;
        }


        /** Writes Key and Certificate to the Location defined in the app settings.
         * Returns False if:
         *      1. One Of the strings is empty
         *      2. Writing failed.
         */
        private static bool WriteKeyCertToFile(String privateKeyString, String certString)
        {
            if (checkCrtOrKey(privateKeyString) && checkCrtOrKey(certString))
            {
                return WriteToFile(DevicePrivateKeyFileLocation, privateKeyString)&&WriteToFile(DeviceCertFileLocation, certString);
            }

            return false;
        }

        /** Checks Certificate and Key.
         * TODO: use openssl to check the validity?
         */ 
        private static bool checkCrtOrKey(string crt)
        {
            return crt.Length != 0;
        }

        /** Writes String to File.
         * Returns true if file exists after writing, otherwise false.
         */
        private static bool WriteToFile(string location,string _string)
        {
            System.IO.File.WriteAllText(location, _string);
            return File.Exists(location);
        }

        /** Checks if a file for Certificate or Key already exitst in their location.
         * 
         */ 
        private static bool checkIfCrtOrKeyExist()
        {
            return File.Exists(DevicePrivateKeyFileLocation) || File.Exists(DeviceCertFileLocation);
        }

      /**Deletes the Credential File>
      * TODO: secure Deletion
      */
        private static bool DeleteCredentials()
        {
            File.Delete(CredentialsFileLocation);
            return !File.Exists(CredentialsFileLocation);
        }
    }
}