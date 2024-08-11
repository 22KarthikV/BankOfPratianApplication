using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using System.Configuration;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class AccountPrivilegeManagerTests
    {
        private Mock<Configuration> _mockConfiguration;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<Configuration>();
            var mockAppSettings = new Mock<AppSettingsSection>();
            var settings = new KeyValueConfigurationCollection();
            settings.Add("DailyLimits", "REGULAR:100000.0;GOLD:200000.0;PREMIUM:300000.0");
            mockAppSettings.Setup(m => m.Settings).Returns(settings);
            _mockConfiguration.Setup(c => c.AppSettings).Returns(mockAppSettings.Object);
        }

        [TestMethod]
        public void Constructor_ValidConfiguration_InitializesDailyLimits()
        {
            // Arrange & Act
            var manager = new AccountPrivilegeManager();

            // Assert
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void GetDailyLimit_ValidPrivilegeType_ReturnsCorrectLimit()
        {
            // Arrange
            var manager = new AccountPrivilegeManager();

            // Act
            var regularLimit = manager.GetDailyLimit(PrivilegeType.REGULAR);
            var goldLimit = manager.GetDailyLimit(PrivilegeType.GOLD);
            var premiumLimit = manager.GetDailyLimit(PrivilegeType.PREMIUM);

            // Assert
            Assert.AreEqual(100000.0, regularLimit);
            Assert.AreEqual(200000.0, goldLimit);
            Assert.AreEqual(300000.0, premiumLimit);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPrivilegeTypeException))]
        public void GetDailyLimit_InvalidPrivilegeType_ThrowsException()
        {
            // Arrange
            var manager = new AccountPrivilegeManager();

            // Act & Assert
            manager.GetDailyLimit((PrivilegeType)999);
        }
    }
}