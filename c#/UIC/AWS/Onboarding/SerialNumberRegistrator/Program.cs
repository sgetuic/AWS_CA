﻿using System;
using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;


namespace SerialNrRegistrator
{

    /** https://eu-central-1.console.aws.amazon.com/dynamodb/home?region=eu-central-1#tables:selected=deviceSerialNumbers;tab=items
     * 
     */
    internal class Program
    {

        private static readonly string SerialNrKeyString = AWSConfigs.GetConfig("SerialNrKeyStr");
        private static readonly string RegisteredKeyString = AWSConfigs.GetConfig("RegisteredKeyStr");
        private static readonly string TimeStampUTCKeyString = AWSConfigs.GetConfig("TimeStampUTCKeyStr");


        private static readonly string ConditionExpressionString = "attribute_not_exists("+ SerialNrKeyString+")";
        
        private static String serialNrInput;

        private static string _tableName = AWSConfigs.GetConfig("TableName");

        private static AmazonDynamoDBClient _dbClient = new AmazonDynamoDBClient();
        
        public static void Main(string[] args)
        {

            while (true)
            {
                
             Console.WriteLine("Enter Serial Number:");
                readSerialNr();

                CreateItem();
            }
 
        }


        private static void readSerialNr()
        {
            serialNrInput = Console.ReadLine();
        }

        /**
         * https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.LowLevelAPI.html
         * 
         */
        private static void CreateItem()
        {
            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    { SerialNrKeyString, new AttributeValue {
                        S = serialNrInput
                    }},
                    { RegisteredKeyString, new AttributeValue {
                        BOOL = false
                        
                    }},
                    {TimeStampUTCKeyString, new AttributeValue
                    {
                        N = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
                    }}
                    
                },
                ConditionExpression = ConditionExpressionString
                
            };
          
            _dbClient.PutItem(request);
        }

    }
}
