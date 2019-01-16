using System;
using System.IO;
using System.Net;
using Amazon;
using Amazon.IoT;
using Amazon.IoT.Model;

/* TODO: If Onboarding Started a second time, CRT and KEY will be overwritten but the device will not be not registered with the new certificates, invalidating the device authentication.
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
        private static string _name = AWSConfigs.GetConfig("DeviceName"); 

        private static string _CRTid;


        public static void Main(string[] args)
        {
            Console.WriteLine($"Loading AWS Credentials");

            _name = $"{_name}{SerialNr}";

            Console.WriteLine("Starting Onboarding");

            CreateCertAndKeys();

            CreateThing();


           if (DeleteCredentials())
               Console.WriteLine("Cerdentials Deleted not safely");
            else
                Console.WriteLine("Credentials not Deleted somehow...");

            Console.WriteLine("Onboarding Finished");
        }


   /* Creates Certificates and Keys in IoT Core and downloads them.
    *     This particular Crt and key combination can be downloaded only once from the IoT Core.
   */


        private static void CreateCertAndKeys()
        {
            CreateKeysAndCertificateRequest request = new CreateKeysAndCertificateRequest {SetAsActive = true};

            CreateKeysAndCertificateResponse response = IoT.CreateKeysAndCertificate(request);

            _CRTid = response.CertificateArn;

            WriteKeyCertToFile(response.KeyPair.PrivateKey, response.CertificatePem);
        }


        /** Creates a thing in IoT Core register.
         *  Serial Number And CRT id will be added as Thing Attributes.
         */
        private static void CreateThing()
        {
            var attributePayload = new AttributePayload();
            attributePayload.Attributes.Add("SerialNr", SerialNr);
            attributePayload.Attributes.Add("CRTId", _CRTid);

            var thingRequest = new CreateThingRequest();
            thingRequest.ThingName = _name;
            thingRequest.AttributePayload = attributePayload;

            var response = IoT.CreateThing(thingRequest);

            Console.WriteLine($"thing creation response: {response.ThingArn}");
            Console.Write("createThing>");
            Console.WriteLine(response.HttpStatusCode == HttpStatusCode.OK);
        }


        private static void WriteKeyCertToFile(String privateKeyString, String certString)
        {
            System.IO.File.WriteAllText(DevicePrivateKeyFileLocation, privateKeyString);
            System.IO.File.WriteAllText(DeviceCertFileLocation, certString);
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