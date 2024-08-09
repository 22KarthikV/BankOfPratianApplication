using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Core;
using System;

namespace BankOfPratian.Core.Tests
{
    [TestClass]
    public class EnumTests
    {
        [TestMethod]
        public void PrivilegeType_HasCorrectValues()
        {
            Assert.AreEqual(3, Enum.GetValues(typeof(PrivilegeType)).Length);
            Assert.IsTrue(Enum.IsDefined(typeof(PrivilegeType), PrivilegeType.REGULAR));
            Assert.IsTrue(Enum.IsDefined(typeof(PrivilegeType), PrivilegeType.GOLD));
            Assert.IsTrue(Enum.IsDefined(typeof(PrivilegeType), PrivilegeType.PREMIUM));
        }

        [TestMethod]
        public void AccountType_HasCorrectValues()
        {
            Assert.AreEqual(2, Enum.GetValues(typeof(AccountType)).Length);
            Assert.IsTrue(Enum.IsDefined(typeof(AccountType), AccountType.SAVINGS));
            Assert.IsTrue(Enum.IsDefined(typeof(AccountType), AccountType.CURRENT));
        }

        [TestMethod]
        public void TransactionType_HasCorrectValues()
        {
            Assert.AreEqual(4, Enum.GetValues(typeof(TransactionType)).Length);
            Assert.IsTrue(Enum.IsDefined(typeof(TransactionType), TransactionType.DEPOSIT));
            Assert.IsTrue(Enum.IsDefined(typeof(TransactionType), TransactionType.WITHDRAW));
            Assert.IsTrue(Enum.IsDefined(typeof(TransactionType), TransactionType.TRANSFER));
            Assert.IsTrue(Enum.IsDefined(typeof(TransactionType), TransactionType.EXTERNALTRANSFER));
        }

        [TestMethod]
        public void TransactionStatus_HasCorrectValues()
        {
            Assert.AreEqual(2, Enum.GetValues(typeof(TransactionStatus)).Length);
            Assert.IsTrue(Enum.IsDefined(typeof(TransactionStatus), TransactionStatus.OPEN));
            Assert.IsTrue(Enum.IsDefined(typeof(TransactionStatus), TransactionStatus.CLOSED));
        }
    }
}