using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using System.Collections.Generic;

namespace BankOfPratian.Business.Tests
{
    [TestClass]
    public class AccountPrivilegeManagerTests
    {
        private AccountPrivilegeManager _accountPrivilegeManager;

        [TestInitialize]
        public void TestInitialize()
        {
            var testDailyLimits = new Dictionary<PrivilegeType, double>
            {
                { PrivilegeType.REGULAR, 100000.0 },
                { PrivilegeType.GOLD, 200000.0 },
                { PrivilegeType.PREMIUM, 300000.0 }
            };

            _accountPrivilegeManager = new AccountPrivilegeManager(testDailyLimits);
        }

        [TestMethod]
        public void GetDailyLimit_RegularPrivilege_ReturnsCorrectLimit()
        {
            // Act
            double limit = _accountPrivilegeManager.GetDailyLimit(PrivilegeType.REGULAR);

            // Assert
            Assert.AreEqual(100000.0, limit);
        }

        [TestMethod]
        public void GetDailyLimit_GoldPrivilege_ReturnsCorrectLimit()
        {
            // Act
            double limit = _accountPrivilegeManager.GetDailyLimit(PrivilegeType.GOLD);

            // Assert
            Assert.AreEqual(200000.0, limit);
        }

        [TestMethod]
        public void GetDailyLimit_PremiumPrivilege_ReturnsCorrectLimit()
        {
            // Act
            double limit = _accountPrivilegeManager.GetDailyLimit(PrivilegeType.PREMIUM);

            // Assert
            Assert.AreEqual(300000.0, limit);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPrivilegeTypeException))]
        public void GetDailyLimit_InvalidPrivilegeType_ThrowsException()
        {
            // Act
            _accountPrivilegeManager.GetDailyLimit((PrivilegeType)999);
        }
    }
}