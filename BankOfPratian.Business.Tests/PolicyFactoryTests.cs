using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using System.Configuration;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class PolicyFactoryTests
    {
        private Mock<Configuration> _mockConfiguration;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<Configuration>();
            var mockAppSettings = new Mock<AppSettingsSection>();
            var settings = new KeyValueConfigurationCollection();
            settings.Add("Policies", "SAVINGS-REGULAR:5000.0,0.04;SAVINGS-GOLD:25000.0,0.0425;CURRENT-REGULAR:25000.0,0.02");
            mockAppSettings.Setup(m => m.Settings).Returns(settings);
            _mockConfiguration.Setup(c => c.AppSettings).Returns(mockAppSettings.Object);
        }

        [TestMethod]
        public void Initialize_ValidConfiguration_CreatesInstance()
        {
            // Act
            PolicyFactory.Initialize(_mockConfiguration.Object);
            var instance = PolicyFactory.Instance;

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CreatePolicy_ValidInput_ReturnsCorrectPolicy()
        {
            // Arrange
            PolicyFactory.Initialize(_mockConfiguration.Object);
            var factory = PolicyFactory.Instance;

            // Act
            var policy = factory.CreatePolicy("SAVINGS", "REGULAR");

            // Assert
            Assert.IsNotNull(policy);
            Assert.AreEqual(5000.0, policy.GetMinBalance());
            Assert.AreEqual(0.04, policy.GetRateOfInterest());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPolicyTypeException))]
        public void CreatePolicy_InvalidInput_ThrowsException()
        {
            // Arrange
            PolicyFactory.Initialize(_mockConfiguration.Object);
            var factory = PolicyFactory.Instance;

            // Act & Assert
            factory.CreatePolicy("INVALID", "TYPE");
        }

        [TestMethod]
        public void GetAllPolicies_ReturnsAllConfiguredPolicies()
        {
            // Arrange
            PolicyFactory.Initialize(_mockConfiguration.Object);
            var factory = PolicyFactory.Instance;

            // Act
            var policies = factory.GetAllPolicies();

            // Assert
            Assert.AreEqual(3, policies.Count);
            Assert.IsTrue(policies.ContainsKey("SAVINGS-REGULAR"));
            Assert.IsTrue(policies.ContainsKey("SAVINGS-GOLD"));
            Assert.IsTrue(policies.ContainsKey("CURRENT-REGULAR"));
        }
    }
}