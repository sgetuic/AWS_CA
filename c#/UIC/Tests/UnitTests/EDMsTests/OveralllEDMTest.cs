using Moq;
using NUnit.Framework;
using System;
using UIC.Framework.Interfaces.Edm;
using UIC.Framework.Interfaces.Edm.Definition;
using UIC.Framework.Interfaces.Edm.Value;
using UIC.Framework.Interfaces.Project;
using UIC.Framework.Interfaces.Util;
using UIC.Framweork.DefaultImplementation;
using UnitTests.FakeEDMs;

namespace UnitTests.EDMsTests
{
    /// <summary>
    /// A summary of Test for every EDM that is using <see cref="InterfaceEmbeddedDriverModule"/> as an interface.
    /// </summary>
    [TestFixture]
    public class OveralllEDMTest
    {
        InterfaceEmbeddedDriverModule _edm;
        Mock<Castle.Core.Logging.ILoggerFactory> loggerFactory = new Mock<Castle.Core.Logging.ILoggerFactory>();
        private string _expectedUri;

        [SetUp]
        public void SetUp()
        {
            // Change the Class to Test a new EDM
            _edm = new FakeEdm(loggerFactory.Object);
            // Change the Uri to suite the EDM
            _expectedUri = "UnitTests.FakeEDMs.FakeEdm";
        }

        [TearDown]
        public void TearDown()
        {
        }

        /// <summary>
        /// Checking EdmHealthStatus after Initialize.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Ini/Dis")]
        public void InitializeTest()
        {
            // Arrange
            string expected = SetEdmStatus(true, false, false);

            // Act
            _edm.Initialize();

            // Assert
            Assert.AreEqual(expected, _edm.GetEdmHealthStatus());
        }

        /// <summary>
        /// Checking EdmHealthStatus after double Initialize.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Ini/Dis")]
        public void DoubleInitializeTest()
        {
            // Arrange
            string expected = SetEdmStatus(true, false, false);

            // Act
            _edm.Initialize();
            _edm.Initialize();

            // Assert
            Assert.AreEqual(expected, _edm.GetEdmHealthStatus());
        }

        /// <summary>
        /// Checking EdmHealthStatus after Dispose then Initialize.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Ini/Dis")]
        public void DisposeThenInitializeTest()
        {
            // Arrange
            string expected = SetEdmStatus(true, false, false);

            // Act
            _edm.Dispose();
            _edm.Initialize();

            // Assert
            Assert.AreEqual(expected, _edm.GetEdmHealthStatus());
        }

        /// <summary>
        /// Checking EdmHealthStatus after Initialize then Dispose.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Ini/Dis")]
        public void InitializeThenDisposeTest()
        {
            // Arrange
            string expected = SetEdmStatus(false, true, false);

            // Act
            _edm.Initialize();
            _edm.Dispose();

            // Assert
            Assert.AreEqual(expected, _edm.GetEdmHealthStatus());
        }

        /// <summary>
        /// Checks IsRunning for expected correctness.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="expected"></param>
        [TestCase(2, true)]
        [TestCase(4, true)]
        [TestCase(8, true)]
        [TestCase(16, true)]
        [TestCase(128, true)]
        [TestCase(2 << 12, true)]
        [TestCase(1024, true)]
        [TestCase(5, false)]
        [Category("EDM")]
        [Category("Running")]
        public void IsRunningTest(int x, bool expected)
        {
            // Act
            var value = _edm.IsRunning(x);

            // Assert
            Assert.AreEqual(expected, value);
        }

        /// <summary>
        /// Checks the EdmCapability for existence of AttributeDefinitions, CommandDefinitions, DatapointDefinitions, Guid and Uri.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Capability")]
        public void GetCapabilityTest()
        {
            // Act
            var value = _edm.GetCapability();

            // Assert
            Assert.NotNull(value);
            Assert.NotNull(value.AttributeDefinitions);
            Assert.NotNull(value.CommandDefinitions);
            Assert.NotNull(value.DatapointDefinitions);
            Assert.NotNull(value.Identifier);
            Assert.IsTrue(TryParseGuid(value.Identifier.Id.ToString()));
            Assert.AreEqual(value.Identifier.Uri, _expectedUri);
        }

        /// <summary>
        /// Sets Command to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Handle")]
        public void HandleNullTest()
        {
            // Act
            try
            {
                var value = _edm.Handle(null);
                // Assert
                Assert.IsTrue(false, "The Handle is suposed to trow an Exception if a null parameter is passed.");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Sets ProjectDatapointTask and Action<DatapointValue> to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("DatapointCallback")]
        public void SetDatapointCallbackNullTest()
        {
            // Act
            try
            {
                _edm.SetDatapointCallback(null, null);
                // Assert
                Assert.IsTrue(false, "An Exception should have been thrown since the ProjectDatapointTask and Action<DatapointValue> were null");
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Sets Action<DatapointValue> to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("DatapointCallback")]
        public void SetDatapointCallbackActionNullTest()
        {
            // Arrange
            // Mocking DatapointDefinition
            var datapointDefinition =
                MockingDatapointDefinition(new Guid("577FEDB1-9D53-421B-9821-104E04D97343"), "DatapointDefinitionLabel", "DatapointDefinition Description", UicDataType.String, _expectedUri);
            // Mocking DatapointTaskReportingCondition
            var datapointTaskReportingCondition =
                MockingDatapointTaskReportingCondition(0, 0, 0);
            // Mocking DatapointTaskMetadata
            var datapointTaskMetadata =
                MockingDatapointTaskMetadata(0, 0, 0, 0, true, "Tags");
            // Setting ProjectDatapointTask
            var projectDatapointTask =
                MockingProjectDatapointTask(0, "ProjectDatapointTask Description", datapointDefinition, datapointTaskReportingCondition, datapointTaskMetadata);

            // Act
            try
            {
                _edm.SetDatapointCallback(projectDatapointTask.Object, null);
                // Assert
                Assert.IsTrue(false, "An Exception should have been thrown since the Action<DatapointValue> was a null");
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Sets ProjectDatapointTask to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("DatapointCallback")]
        public void SetDatapointProjectDatapointTaskNullTest()
        {
            // Arrange
            Action<DatapointValue> actionDatapointValue = (x) => Console.WriteLine(x.Value);

            // Act
            try
            {
                _edm.SetDatapointCallback(null, actionDatapointValue);
                // Assert
                Assert.IsTrue(false, "An Exception should have been thrown since the ProjectDatapointTask was a null");
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Sets AttributeDefinition and Action<AttributeValue> to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("AttributeCallback")]
        public void SetAttributeCallbackNullTest()
        {
            // Act
            try
            {
                _edm.SetAttributeCallback(null, null);
                // Assert
                Assert.IsTrue(false, "An Exception should have been thrown since the AttributeDefinition and Action<AttributeValue> were null");
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Sets Action<AttributeValue> to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("AttributeCallback")]
        public void SetAttributeCallbackActionNullTest()
        {
            // Arrange
            // Needs to be mocked or create new depending on the EDM.
            var attributeDefinition = MockingAttributeDefinition(new Guid("577FEDB1-9D53-421B-9821-104E04D97343"), "AttributeDefinitionLabel", "AttributeDefinition Description", UicDataType.String, _expectedUri);

            // Act
            try
            {
                _edm.SetAttributeCallback(attributeDefinition.Object, null);
                // Assert
                Assert.IsTrue(false, "An Exception should have been thrown since the Action<AttributeValue> was a null");
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Sets AttributeDefinition to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("AttributeCallback")]
        public void SetAttributeCallbackAttributeDefinitionNullTest()
        {
            // Arrange
            Action<AttributeValue> actionAttributeValue = (x) => Console.WriteLine(x.Value);

            // Act
            try
            {
                _edm.SetAttributeCallback(null, actionAttributeValue);
                // Assert
                Assert.IsTrue(false, "An Exception should have been thrown since the AttributeDefinition was a null");
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Sets DatapointDefinition to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("GetValueFor")]
        public void GetValueForDatapointDefinitionNullTest()
        {
            // Arrange 
            SgetDatapointDefinition datapointDefinition = null;
            DatapointValue value = null;
            // Act
            try
            {
                value = _edm.GetValueFor(datapointDefinition);
                // Assert
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
            Assert.IsNull(value);
        }

        /// <summary>
        /// Sets AttributeDefinition to null and expecting an exception.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("GetValueFor")]
        public void GetValueForAttributeDefinitionNullTest()
        {
            // Arrange 
            SgetAttributDefinition attributeDefinition = null;
            AttributeValue value = null;
            // Act
            try
            {
                value = _edm.GetValueFor(attributeDefinition);
                // Assert
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
            Assert.IsNull(value);
        }

        /// <summary>
        /// Checks the Identifier for Uri and Guid.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Identifier")]
        public void IdentifierTest()
        {
            // Act
            var value = _edm.Identifier;

            // Assert
            Assert.NotNull(value);
            Assert.AreEqual(value.Uri, _expectedUri);
            Assert.IsTrue(TryParseGuid(value.Id.ToString()));
        }

        /// <summary>
        /// Tries to parse a Guid to ensure it is a correct Guid.
        /// </summary>
        /// <param name="guidString">Guid to be tested.</param>
        /// <returns>true if it is a correct Guid, false if not.</returns>
        private bool TryParseGuid(string guidString)
        {
            if (guidString == null) return false;
            Guid guid;
            try
            {
                guid = new Guid(guidString);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the Edm Status string so it could be tested easier.
        /// </summary>
        /// <param name="initDone">Boolean that is representing if the initialization was finished.</param>
        /// <param name="isDispose">Boolean that is representing if the dispose was successful.</param>
        /// <param name="initFailed">Boolean that is representing if the initialization failed to initialize.</param>
        /// <returns>String representing the EDM Status.</returns>
        private string SetEdmStatus(bool initDone, bool isDispose, bool initFailed)
        {
            return "Status:\n" +
                "initDone = " + initDone.ToString() + "\n" +
                "isDispose = " + isDispose.ToString() + "\n" +
                "initFailed = " + initFailed.ToString() + "";
        }

        /// <summary>
        /// A function to allow simpler mocking of ProjectDatapointTask
        /// </summary>
        /// <param name="pollIntervall"></param>
        /// <param name="description"></param>
        /// <param name="datapointDefinition"><see cref="MockingDatapointDefinition"/></param>
        /// <param name="datapointTaskReportingCondition"><see cref="MockingDatapointTaskReportingCondition"/></param>
        /// <param name="datapointTaskMetadata"><see cref="MockingDatapointTaskMetadata"/></param>
        /// <returns>Mocked ProjectDatapointTask</returns>
        private Mock<ProjectDatapointTask> MockingProjectDatapointTask(int pollIntervall, string description, Mock<DatapointDefinition> datapointDefinition, Mock<DatapointTaskReportingCondition> datapointTaskReportingCondition, Mock<DatapointTaskMetadata> datapointTaskMetadata)
        {
            var projectDatapointTask = new Mock<ProjectDatapointTask>();
            // Setting PollIntervall
            projectDatapointTask
                .Setup(pdt => pdt.PollIntervall)
                .Returns(pollIntervall);
            // Setting Description
            projectDatapointTask
                .Setup(pdt => pdt.Description)
                .Returns("ProjectDatapointTask Description");
            // Setting mocked Definition
            projectDatapointTask
                .Setup(pdt => pdt.Definition)
                .Returns(datapointDefinition.Object);
            // Setting ReportingCondition
            projectDatapointTask
                .Setup(pdt => pdt.ReportingCondition)
                .Returns(datapointTaskReportingCondition.Object);
            // Setting PollIntervall
            projectDatapointTask
                .Setup(pdt => pdt.MetaData)
                .Returns(datapointTaskMetadata.Object);
            return projectDatapointTask;
        }

        /// <summary>
        /// A function to allow simpler mocking of DatapointTaskMetadata
        /// </summary>
        /// <param name="expectedMaximum"></param>
        /// <param name="expectedMinimum"></param>
        /// <param name="warningThreshold"></param>
        /// <param name="errorThreshold"></param>
        /// <param name="islnverseThresholdEvaluation"></param>
        /// <param name="tags"></param>
        /// <returns>Mocked DatapointTaskMetadata</returns>
        private Mock<DatapointTaskMetadata> MockingDatapointTaskMetadata(double expectedMaximum, double expectedMinimum, double warningThreshold, double errorThreshold, bool islnverseThresholdEvaluation, string tags)
        {
            var datapointTaskMetadata = new Mock<DatapointTaskMetadata>();
            // Setting ExpectedMaximum
            datapointTaskMetadata
                .Setup(dtm => dtm.ExpectedMaximum)
                .Returns(expectedMaximum);
            // Setting ExpectedMinimum
            datapointTaskMetadata
                .Setup(dtm => dtm.ExpectedMinimum)
                .Returns(expectedMinimum);
            // Setting WarningThreshold
            datapointTaskMetadata
                .Setup(dtm => dtm.WarningThreshold)
                .Returns(warningThreshold);
            // Setting ErrorThreshold
            datapointTaskMetadata
                .Setup(dtm => dtm.ErrorThreshold)
                .Returns(errorThreshold);
            // Setting IslnverseThresholdEvaluation
            datapointTaskMetadata
                .Setup(dtm => dtm.IslnverseThresholdEvaluation)
                .Returns(islnverseThresholdEvaluation);
            // Setting Tags
            datapointTaskMetadata
                .Setup(dtm => dtm.Tags)
                .Returns(tags);
            return datapointTaskMetadata;
        }

        /// <summary>
        /// A function to allow simpler mocking of DatapointTaskReportingCondition
        /// </summary>
        /// <param name="reportingThreshoIdInpercent"></param>
        /// <param name="minimalAhsoluteChange"></param>
        /// <param name="reportingThresholdInMilliSecs"></param>
        /// <returns>Mocked DatapointTaskReportingCondition</returns>
        private Mock<DatapointTaskReportingCondition> MockingDatapointTaskReportingCondition(double reportingThreshoIdInpercent, double minimalAhsoluteChange, long reportingThresholdInMilliSecs)
        {
            var datapointTaskReportingCondition = new Mock<DatapointTaskReportingCondition>();
            // Setting ReportingThreshoIdInpercent
            datapointTaskReportingCondition
                .Setup(dtrc => dtrc.ReportingThreshoIdInpercent)
                .Returns(reportingThreshoIdInpercent);
            // Setting MinimalAhsoluteChange
            datapointTaskReportingCondition
                .Setup(dtrc => dtrc.MinimalAhsoluteChange)
                .Returns(minimalAhsoluteChange);
            // Setting ReportingThreshoIdInpercent
            datapointTaskReportingCondition
                .Setup(dtrc => dtrc.ReportingThresholdInMilliSecs)
                .Returns(reportingThresholdInMilliSecs);
            return datapointTaskReportingCondition;
        }

        /// <summary>
        /// A function to allow simpler mocking of DatapointDefinition
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="label"></param>
        /// <param name="description"></param>
        /// <param name="dataType"></param>
        /// <param name="uri"></param>
        /// <returns>Mocked DatapointDefinition</returns>
        private Mock<DatapointDefinition> MockingDatapointDefinition(Guid guid, string label, string description, UicDataType dataType, string uri)
        {
            var datapointDefinition = new Mock<DatapointDefinition>();
            // Setting Id
            datapointDefinition
                .Setup(dd => dd.Id)
                .Returns(guid);
            // Setting Label
            datapointDefinition
                .Setup(dd => dd.Label)
                .Returns(label);
            // Setting Description
            datapointDefinition
                .Setup(dd => dd.Description)
                .Returns(description);
            // Setting DataType
            datapointDefinition
                .Setup(dd => dd.DataType)
                .Returns(dataType);
            // Setting Uri
            datapointDefinition
                .Setup(dd => dd.Uri)
                .Returns(uri);
            return datapointDefinition;
        }

        /// <summary>
        /// A function to allow simpler mocking of AttributeDefinition
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="label"></param>
        /// <param name="description"></param>
        /// <param name="dataType"></param>
        /// <param name="uri"></param>
        /// <returns>Mocked AttributeDefinition</returns>
        private Mock<AttributeDefinition> MockingAttributeDefinition(Guid guid, string label, string description, UicDataType dataType, string uri)
        {
            var attributeDefinition = new Mock<AttributeDefinition>();
            // Setting Guid
            attributeDefinition
                .Setup(ad => ad.Id)
                .Returns(guid);
            // Setting Label
            attributeDefinition
                .Setup(ad => ad.Label)
                .Returns(label);
            // Setting Description
            attributeDefinition
                .Setup(ad => ad.Description)
                .Returns(description);
            // Setting DataType
            attributeDefinition
                .Setup(ad => ad.DataType)
                .Returns(dataType);
            // Setting Uri
            attributeDefinition
                .Setup(ad => ad.Uri)
                .Returns(uri);
            return attributeDefinition;
        }

//-----------------------------------------------------------------------------------------------
//-----------------------Test-Specific-for-the-class-FakeEDM-------------------------------------
//-----------------------------------------------------------------------------------------------

        /// <summary>
        /// Mocking a Command varablie to return true.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Handle")]
        public void HandleCommandTrueTest()
        {
            // Act
            bool value = false;
            try
            {
                var command = new Mock<Command>();
                command
                    .Setup(c => c.CommandDefinition.Command)
                    .Returns("Hi");
                value = _edm.Handle(command.Object);
                // Assert
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, e.Message);
            }
            Assert.IsTrue(value);
        }

        /// <summary>
        /// Mocking a Command varablie to return false.
        /// </summary>
        [Test]
        [Category("EDM")]
        [Category("Handle")]
        public void HandleCommandFalseTest()
        {
            // Arrange
            bool value = true;

            // Act
            try
            {
                var command = new Mock<Command>();
                command
                    .Setup(c => c.CommandDefinition.Command)
                    .Returns("Bye");
                value = _edm.Handle(command.Object);
                // Assert
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, e.Message);
            }
            Assert.IsFalse(value);
        }
    }
}
