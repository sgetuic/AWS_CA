using System;
using System.IO;
using System.Net;
using Amazon;
using Amazon.IoT;
using Amazon.IoT.Model;

/* This application does devices onboarding with the AWS IoT Core. It was made to be run during the final steps of a device manufacturing, where a general credentials file is provided for a one time use,
 and should be removed after onboarding has been done.
 
 It accomplishes 2 things:
 1.Downloading a Certificate and a private key pair from the AWS IoT.
 2.Creates a "Thing" entry in the IoT Core Management with the id for it's certificate.
 
 The rest of the onboarding currently is done with a Lambda Service. See Communication.AWS.Onboarding.Lambda_registration_finishing.
 */
namespace UIC.AWS.Onboarding.Device
{
    internal class Onboard
    {
        private static readonly IAmazonIoT IoT = new AmazonIoTClient();

        private static readonly string DeviceCertAndKeyLocation     = AWSConfigs.GetConfig("DeviceCertAndKeySaveLocation");
        private static readonly string DeviceCertFileLocation       = DeviceCertAndKeyLocation + AWSConfigs.GetConfig("DeviceCrtFileName");
        private static readonly string DevicePrivateKeyFileLocation = DeviceCertAndKeyLocation + AWSConfigs.GetConfig("DeviceKeyFileName");
        private static readonly string CredentialsFileLocation      = AWSConfigs.AWSProfilesLocation;
        private static readonly string SerialNr                     = AWSConfigs.GetConfig("DeviceSerialNumber");

        /**Device name. Will be concatenated with SerialNr */
        private static readonly string DeviceName                   = AWSConfigs.GetConfig("DeviceName") + SerialNr;
        
        private static string _CRTid;


        public static void Main(string[] args)
        {
           
            if (checkIfCrtOrKeyExist())
            {
                Console.WriteLine("There already Exists a key or crt file! Onboarding not started.");
                return;
            }

            Console.WriteLine("Starting Onboarding...");

            if (!CreateCertAndKeys())
            {
                Console.WriteLine("Couldn't create/cownload certificate and/or keys.");
                return;
            }


            if (!CreateThing())
            {
                Console.WriteLine("Couldn't register the thing on the AWS IoT core.");
                return;
            }


            if (DeleteCredentials())
            {
                Console.WriteLine("WARNING: CREDENTIALS DELETED NOT SAFELY!");
            }
            else
            {
                Console.WriteLine("WARNING: CREDENTIALS FILE HAVE NOT BEEN DELETED .");
            }


            Console.WriteLine("Onboarding finished successfully!");
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


        /**AWS IoT core makes and sends Certificate and Keys pair.
         *
         */
        private static CreateKeysAndCertificateResponse getKeyAndCertResponse()
        {
            CreateKeysAndCertificateRequest request = new CreateKeysAndCertificateRequest {SetAsActive = true};
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

            return (response.HttpStatusCode == HttpStatusCode.OK);
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
                return WriteToFile(DevicePrivateKeyFileLocation, privateKeyString) &&
                       WriteToFile(DeviceCertFileLocation, certString);
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
        private static bool WriteToFile(string location, string _string)
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
            // File.Delete(CredentialsFileLocation);
            return !File.Exists(CredentialsFileLocation);
        }
    }
}