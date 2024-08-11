using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using Moq;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class ExternalBankServiceFactoryTests
    {
        private Mock<IExternalBankService> _mockExternalBankService;

        [TestInitialize]
        public void Setup()
        {
            _mockExternalBankService = new Mock<IExternalBankService>();

            // Setup mock configuration
            var configFile = new ExeConfigurationFileMap { ExeConfigFilename = "TestApp.config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
            config.AppSettings.Settings.Add("ServiceBanks", "ICICI:BankOfPratian.Business.ICICIBankService;CITI:BankOfPratian.Business.CITIBankService");
            ConfigurationManager.RefreshSection("appSettings");
        }

        [TestMethod]
        public void GetExternalBankService_ValidBankCode_ReturnsService()
        {
            // Arrange
            var factory = ExternalBankServiceFactory.Instance;

            // Act
            var service = factory.GetExternalBankService("ICICI");

            // Assert
            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(IExternalBankService));
        }

        [TestMethod]
        [ExpectedException(typeof(AccountDoesNotExistException))]
        public void GetExternalBankService_InvalidBankCode_ThrowsException()
        {
            // Arrange
            var factory = ExternalBankServiceFactory.Instance;

            // Act & Assert
            factory.GetExternalBankService("INVALID");
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Constructor_MissingConfiguration_ThrowsException()
        {
            // Arrange
            var configFile = new ExeConfigurationFileMap { ExeConfigFilename = "EmptyConfig.config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection("appSettings");

            // Act & Assert
            var factory = ExternalBankServiceFactory.Instance;
        }

        [TestMethod]
        public void Constructor_InvalidConfigurationEntry_LogsWarning()
        {
            // Arrange
            var configFile = new ExeConfigurationFileMap { ExeConfigFilename = "InvalidConfig.config" };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
            config.AppSettings.Settings.Add("ServiceBanks", "INVALID_ENTRY");
            ConfigurationManager.RefreshSection("appSettings");

            // Act
            var factory = ExternalBankServiceFactory.Instance;

            // Assert
            // Check logs for warning message
            // This would require a mock logger or a test logger to verify
        }
    }
}