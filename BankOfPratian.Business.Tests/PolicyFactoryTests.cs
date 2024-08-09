using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using System.Configuration;

namespace BankOfPratian.Business.Tests
{
    [TestClass]
    public class PolicyFactoryTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            // Set up the configuration for testing
            ConfigurationManager.AppSettings["Policies"] = "SAVINGS-REGULAR:5000.0,4.0;CURRENT-PREMIUM:300000.0,2.75";
        }

        [TestMethod]
        public void CreatePolicy_ValidInput_ReturnsCorrectPolicy()
        {
            // Arrange
            string accType = "SAVINGS";
            string privilege = "REGULAR";

            // Act
            IPolicy policy = PolicyFactory.Instance.CreatePolicy(accType, privilege);

            // Assert
            Assert.IsNotNull(policy);
            Assert.AreEqual(5000.0, policy.GetMinBalance());
            Assert.AreEqual(4.0, policy.GetRateOfInterest());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPolicyTypeException))]
        public void CreatePolicy_InvalidInput_ThrowsException()
        {
            // Arrange
            string accType = "INVALID";
            string privilege = "INVALID";

            // Act
            PolicyFactory.Instance.CreatePolicy(accType, privilege);
        }
    }
}