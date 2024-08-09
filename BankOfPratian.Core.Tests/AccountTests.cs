using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Core;
using System;
using BankOfPratian.Business;

namespace BankOfPratian.Core.Tests
{
    [TestClass]
    public class AccountTests
    {
        [TestMethod]
        public void SavingsAccount_Constructor_SetsCorrectAccountType()
        {
            var account = new SavingsAccount();
            Assert.AreEqual(AccountType.SAVINGS, account.GetAccType());
        }

        [TestMethod]
        public void CurrentAccount_Constructor_SetsCorrectAccountType()
        {
            var account = new CurrentAccount();
            Assert.AreEqual(AccountType.CURRENT, account.GetAccType());
        }

        [TestMethod]
        public void Account_Open_SetsActiveToTrueAndDateOfOpening()
        {
            var account = new SavingsAccount();
            var beforeOpen = DateTime.Now;
            bool result = account.Open();
            var afterOpen = DateTime.Now;

            Assert.IsTrue(result);
            Assert.IsTrue(account.Active);
            Assert.IsTrue(account.DateOfOpening >= beforeOpen && account.DateOfOpening <= afterOpen);
        }

        [TestMethod]
        public void Account_Close_SetsActiveToFalseAndBalanceToZero()
        {
            var account = new CurrentAccount();
            account.Open();
            account.Balance = 1000;

            bool result = account.Close();

            Assert.IsTrue(result);
            Assert.IsFalse(account.Active);
            Assert.AreEqual(0, account.Balance);
        }

        [TestMethod]
        public void Account_AccNo_IsReadOnly()
        {
            // Arrange
            var account = new SavingsAccount();
            string initialAccNo = account.AccNo;

            // Act & Assert
            var propertyInfo = typeof(IAccount).GetProperty("AccNo");
            Assert.IsNotNull(propertyInfo, "AccNo property not found");
            Assert.IsFalse(propertyInfo.CanWrite, "AccNo property should not have a setter");

            // Verify AccNo hasn't changed
            Assert.AreEqual(initialAccNo, account.AccNo);
        }

        [TestMethod]
        public void Account_SetBalance_UpdatesBalanceCorrectly()
        {
            var account = new SavingsAccount();
            double newBalance = 5000;

            account.Balance = newBalance;

            Assert.AreEqual(newBalance, account.Balance);
        }

        [TestMethod]
        public void Account_SetPrivilegeType_UpdatesPrivilegeTypeCorrectly()
        {
            var account = new CurrentAccount();
            PrivilegeType newPrivilegeType = PrivilegeType.GOLD;

            account.PrivilegeType = newPrivilegeType;

            Assert.AreEqual(newPrivilegeType, account.PrivilegeType);
        }

        [TestMethod]
        public void Account_SetPin_UpdatesPinCorrectly()
        {
            var account = new SavingsAccount();
            string newPin = "1234";

            account.Pin = newPin;

            Assert.AreEqual(newPin, account.Pin);
        }

        [TestMethod]
        public void Account_SetName_UpdatesNameCorrectly()
        {
            var account = new CurrentAccount();
            string newName = "John Doe";

            account.Name = newName;

            Assert.AreEqual(newName, account.Name);
        }

        [TestMethod]
        public void Account_SetPolicy_UpdatesPolicyCorrectly()
        {
            var account = new SavingsAccount();
            IPolicy newPolicy = new Policy(1000, 2.5);

            account.Policy = newPolicy;

            Assert.AreEqual(newPolicy, account.Policy);
        }

        [TestMethod]
        public void Account_Constructor_GeneratesUniqueAccNo()
        {
            var account1 = new SavingsAccount();
            var account2 = new SavingsAccount();

            Assert.AreNotEqual(account1.AccNo, account2.AccNo);
        }
    }
}