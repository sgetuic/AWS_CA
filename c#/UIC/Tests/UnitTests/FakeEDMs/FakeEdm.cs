using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using UIC.EDM.EApi.I2c;
using UIC.Framework.Interfaces.Edm;
using UIC.Framework.Interfaces.Edm.Definition;
using UIC.Framework.Interfaces.Edm.Value;
using UIC.Framework.Interfaces.Project;
using UIC.Framework.Interfaces.Util;
using UIC.Framweork.DefaultImplementation;
using UnitTests.FakeEapiInitializers;
using UIC.Framework.Interfaces.Eapi;

namespace UnitTests.FakeEDMs
{
    internal class FakeEdm : InterfaceEmbeddedDriverModule
    {
        private readonly IEapiInitializer _eapiInitializer;
        private readonly EdmCapability _edmCapability;
        private readonly Guid _maxblockLengthAttributeId = new Guid("{16c89a68-a270-489b-affb-2f9c1c43902e}");

        public EdmIdentifier Identifier { get; }


        public FakeEdm(ILoggerFactory loggerFactory)
        {
            Identifier = new EapiI2cEdmIdentifier(GetType().FullName);
            _eapiInitializer = new FakeEapiInitializer();
            _edmCapability = CreateEdmCapability();
        }

        private EdmCapability CreateEdmCapability()
        {
            List<AttributeDefinition> attribtueDefinitions = new List<AttributeDefinition>
            {
                new SgetAttributDefinition(
                    _maxblockLengthAttributeId,
                    UicUriBuilder.DatapointFrom(this, "HelloWorld"),
                    "Hello World",
                    UicDataType.String,
                    "It says Hello World")
            };
            return new EapI2cEdmCapability(Identifier, new CommandDefinition[0], attribtueDefinitions.ToArray(), new DatapointDefinition[0]);
        }

        public void Initialize()
        {
            _eapiInitializer.Init();
        }

        public void Dispose()
        {
            _eapiInitializer.Dispose();
        }

        public EdmCapability GetCapability()
        {
            return _edmCapability;
        }

        public DatapointValue GetValueFor(DatapointDefinition datapoint)
        {
            if (datapoint == null)
                throw new ArgumentException("DatapointDefinition parameter is null");
            throw new ArgumentException("Unknown DatapointDefinition: " + datapoint);
        }

        public AttributeValue GetValueFor(AttributeDefinition attribute)
        {
            if (attribute == null)
                throw new ArgumentException("AttributeDefinition parameter is null");
            return new SgetAttributeValue("Hello World", attribute);
        }

        public bool Handle(Command command)
        {
            if (command == null)
                throw new ArgumentException("Command parameter is null");
            if (command.CommandDefinition.Command.Contains("Hi"))
                return true;

            return false;
        }

        public void SetDatapointCallback(ProjectDatapointTask datapointTask, Action<DatapointValue> callback)
        {
            if(datapointTask == null || callback == null)
                throw new ArgumentException("ProjectDatapointTask or Action<DatapointValue> parameter is null");
            // no need for callbacks
        }

        public void SetAttributeCallback(AttributeDefinition attributeDefinition, Action<AttributeValue> callback)
        {
            if (attributeDefinition == null || callback == null)
                throw new ArgumentException("AttributeDefinition or Action<AttributeValue> parameter is null");
            // no need for callbacks
        }

        public string GetEdmHealthStatus()
        {
            return SetEdmStatus(_eapiInitializer.GetInitDone(),
                _eapiInitializer.GetDispose(),
                _eapiInitializer.GetInitFailed());
        }

        private string SetEdmStatus(bool initDone, bool isDispose, bool initFailed)
        {
            return "Status:\n" +
                "initDone = " + initDone.ToString() + "\n" +
                "isDispose = " + isDispose.ToString() + "\n" +
                "initFailed = " + initFailed.ToString() + "";
        }

        public bool IsRunning(int x)
        {
            if ((x & -x) == x)
                return true;
            return false;
        }
    }
}
