using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Core;
using BankOfPratian.Business;

namespace BankOfPratian.Core.Tests
{
    [TestClass]
    public class PolicyTests
    {
        [TestMethod]
        public void Policy_Constructor_SetsCorrectValues()
        {
            double expectedMinBalance = 5000;
            double expectedRateOfInterest = 4.5;

            var policy = new Policy(expectedMinBalance, expectedRateOfInterest);

            Assert.AreEqual(expectedMinBalance, policy.GetMinBalance());
            Assert.AreEqual(expectedRateOfInterest, policy.GetRateOfInterest());
        }

        [TestMethod]
        public void Policy_GetMinBalance_ReturnsCorrectValue()
        {
            double expectedMinBalance = 10000;
            var policy = new Policy(expectedMinBalance, 3.5);

            double actualMinBalance = policy.GetMinBalance();

            Assert.AreEqual(expectedMinBalance, actualMinBalance);
        }

        [TestMethod]
        public void Policy_GetRateOfInterest_ReturnsCorrectValue()
        {
            double expectedRateOfInterest = 3.75;
            var policy = new Policy(5000, expectedRateOfInterest);

            double actualRateOfInterest = policy.GetRateOfInterest();

            Assert.AreEqual(expectedRateOfInterest, actualRateOfInterest);
        }

        [TestMethod]
        public void Policy_Constructor_HandlesZeroMinBalance()
        {
            double expectedMinBalance = 0;
            double expectedRateOfInterest = 1.0;

            var policy = new Policy(expectedMinBalance, expectedRateOfInterest);

            Assert.AreEqual(expectedMinBalance, policy.GetMinBalance());
        }

        [TestMethod]
        public void Policy_Constructor_HandlesZeroRateOfInterest()
        {
            double expectedMinBalance = 1000;
            double expectedRateOfInterest = 0;

            var policy = new Policy(expectedMinBalance, expectedRateOfInterest);

            Assert.AreEqual(expectedRateOfInterest, policy.GetRateOfInterest());
        }

        [TestMethod]
        public void Policy_Constructor_HandlesNegativeMinBalance()
        {
            double expectedMinBalance = -1000;
            double expectedRateOfInterest = 2.5;

            var policy = new Policy(expectedMinBalance, expectedRateOfInterest);

            Assert.AreEqual(expectedMinBalance, policy.GetMinBalance());
        }

        [TestMethod]
        public void Policy_Constructor_HandlesNegativeRateOfInterest()
        {
            double expectedMinBalance = 1000;
            double expectedRateOfInterest = -2.5;

            var policy = new Policy(expectedMinBalance, expectedRateOfInterest);

            Assert.AreEqual(expectedRateOfInterest, policy.GetRateOfInterest());
        }
    }
}