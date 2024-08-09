/*using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.DataAccess;
using System;
using System.Collections.Generic;

namespace BankOfPratian.Business.Tests
{
    [TestClass]
    public class AccountManagerTests
    {
        private AccountPrivilegeManager _privilegeManager;
        private Mock<IAccountDAO> _mockAccountDAO;
        private Mock<ITransactionDAO> _mockTransactionDAO;
        private Mock<IPolicyFactory> _mockPolicyFactory;
        private AccountManager _accountManager;

        [TestInitialize]
        public void TestInitialize()
        {
            var dailyLimits = new Dictionary<PrivilegeType, double>
            {
                { PrivilegeType.REGULAR, 100000.0 },
                { PrivilegeType.GOLD, 200000.0 },
                { PrivilegeType.PREMIUM, 300000.0 }
            };
            _privilegeManager = new AccountPrivilegeManager(dailyLimits);
            _mockAccountDAO = new Mock<IAccountDAO>();
            _mockTransactionDAO = new Mock<ITransactionDAO>();
            _mockPolicyFactory = new Mock<IPolicyFactory>();

            _accountManager = new AccountManager(_privilegeManager, _mockAccountDAO.Object, _mockTransactionDAO.Object, _mockPolicyFactory.Object);

            _mockTransactionDAO.Setup(td => td.GetDailyTransferAmount(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(0);
        }

        [TestMethod]
        public void CreateAccount_ValidInput_ReturnsNewAccount()
        {
            // Arrange
            string name = "John Doe";
            string pin = "1234";
            double balance = 1000;
            PrivilegeType privilegeType = PrivilegeType.REGULAR;
            AccountType accountType = AccountType.SAVINGS;

            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(500);
            mockPolicy.Setup(p => p.GetRateOfInterest()).Returns(4.0);

            _mockPolicyFactory.Setup(pf => pf.CreatePolicy(accountType.ToString(), privilegeType.ToString())).Returns(mockPolicy.Object);

            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Name).Returns(name);
            mockAccount.Setup(a => a.Pin).Returns(pin);
            mockAccount.Setup(a => a.Balance).Returns(balance);
            mockAccount.Setup(a => a.PrivilegeType).Returns(privilegeType);
            mockAccount.Setup(a => a.GetAccType()).Returns(accountType);
            mockAccount.Setup(a => a.Open()).Returns(true);

            _mockAccountDAO.Setup(dao => dao.CreateAccount(It.IsAny<IAccount>()))
                .Callback<IAccount>(account =>
                {
                    // Here we can verify the properties of the account being created
                    Assert.AreEqual(name, account.Name);
                    Assert.AreEqual(pin, account.Pin);
                    Assert.AreEqual(balance, account.Balance);
                    Assert.AreEqual(privilegeType, account.PrivilegeType);
                    Assert.AreEqual(accountType, account.GetAccType());
                });

            // Act
            var result = _accountManager.CreateAccount(name, pin, balance, privilegeType, accountType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(pin, result.Pin);
            Assert.AreEqual(balance, result.Balance);
            Assert.AreEqual(privilegeType, result.PrivilegeType);
            Assert.AreEqual(accountType, result.GetAccType());
            _mockAccountDAO.Verify(dao => dao.CreateAccount(It.IsAny<IAccount>()), Times.Once);
            _mockPolicyFactory.Verify(pf => pf.CreatePolicy(accountType.ToString(), privilegeType.ToString()), Times.Once);
        }

        [TestMethod]
        public void Deposit_ValidInput_UpdatesBalance()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(true);
            mockAccount.Setup(a => a.Balance).Returns(1000);
            double depositAmount = 500;

            // Act
            var result = _accountManager.Deposit(mockAccount.Object, depositAmount);

            // Assert
            Assert.IsTrue(result);
            mockAccount.VerifySet(a => a.Balance = 1500);
            _mockAccountDAO.Verify(dao => dao.UpdateAccount(It.IsAny<IAccount>()), Times.Once);
            _mockTransactionDAO.Verify(dao => dao.LogTransaction(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        public void Withdraw_ValidInput_UpdatesBalance()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(true);
            mockAccount.Setup(a => a.Balance).Returns(1000);
            mockAccount.Setup(a => a.Pin).Returns("1234");
            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(100);
            mockAccount.Setup(a => a.Policy).Returns(mockPolicy.Object);

            double withdrawAmount = 500;

            // Act
            var result = _accountManager.Withdraw(mockAccount.Object, withdrawAmount, "1234");

            // Assert
            Assert.IsTrue(result);
            mockAccount.VerifySet(a => a.Balance = 500);
            _mockAccountDAO.Verify(dao => dao.UpdateAccount(It.IsAny<IAccount>()), Times.Once);
            _mockTransactionDAO.Verify(dao => dao.LogTransaction(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        public void TransferFunds_ValidInput_UpdatesBalances()
        {
            // Arrange
            var mockFromAccount = new Mock<IAccount>();
            mockFromAccount.Setup(a => a.Active).Returns(true);
            mockFromAccount.Setup(a => a.Balance).Returns(1000);
            mockFromAccount.Setup(a => a.Pin).Returns("1234");
            mockFromAccount.Setup(a => a.PrivilegeType).Returns(PrivilegeType.REGULAR);

            var mockToAccount = new Mock<IAccount>();
            mockToAccount.Setup(a => a.Active).Returns(true);
            mockToAccount.Setup(a => a.Balance).Returns(500);

            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(100);
            mockFromAccount.Setup(a => a.Policy).Returns(mockPolicy.Object);

            var transfer = new Transfer
            {
                FromAcc = mockFromAccount.Object,
                ToAcc = mockToAccount.Object,
                Amount = 300,
                Pin = "1234"
            };

            _mockTransactionDAO.Setup(td => td.GetDailyTransferAmount(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(0);

            // Act
            var result = _accountManager.TransferFunds(transfer);

            // Assert
            Assert.IsTrue(result);
            mockFromAccount.VerifySet(a => a.Balance = 700);
            mockToAccount.VerifySet(a => a.Balance = 800);
            _mockAccountDAO.Verify(dao => dao.UpdateAccount(It.IsAny<IAccount>()), Times.Exactly(2));
            _mockTransactionDAO.Verify(dao => dao.LogTransaction(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        public void GetAccount_ValidAccountNumber_ReturnsAccount()
        {
            // Arrange
            string accNo = "ACC123";
            var mockAccount = new Mock<IAccount>();
            _mockAccountDAO.Setup(dao => dao.GetAccount(accNo)).Returns(mockAccount.Object);

            // Act
            var result = _accountManager.GetAccount(accNo);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockAccount.Object, result);
            _mockAccountDAO.Verify(dao => dao.GetAccount(accNo), Times.Once);
        }
    }
}
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.DataAccess;
using System;

namespace BankOfPratian.Business.Tests
{
    [TestClass]
    public class AccountManagerTests
    {
        private Mock<AccountPrivilegeManager> _mockPrivilegeManager;
        private Mock<IAccountDAO> _mockAccountDAO;
        private Mock<ITransactionDAO> _mockTransactionDAO;
        private AccountManager _accountManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockPrivilegeManager = new Mock<AccountPrivilegeManager>();
            _mockAccountDAO = new Mock<IAccountDAO>();
            _mockTransactionDAO = new Mock<ITransactionDAO>();
            _accountManager = new AccountManager(_mockPrivilegeManager.Object, _mockAccountDAO.Object, _mockTransactionDAO.Object);
        }

        [TestMethod]
        public void Deposit_ValidInput_UpdatesBalance()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(true);
            mockAccount.Setup(a => a.Balance).Returns(1000);
            mockAccount.SetupProperty(a => a.Balance);
            double depositAmount = 500;

            // Act
            _accountManager.Deposit(mockAccount.Object, depositAmount);

            // Assert
            Assert.AreEqual(1500, mockAccount.Object.Balance);
            _mockAccountDAO.Verify(dao => dao.UpdateAccount(It.IsAny<IAccount>()), Times.Once);
            _mockTransactionDAO.Verify(dao => dao.LogTransaction(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InactiveAccountException))]
        public void Deposit_InactiveAccount_ThrowsException()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(false);

            // Act
            _accountManager.Deposit(mockAccount.Object, 500);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void Withdraw_ValidInput_UpdatesBalance()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(true);
            mockAccount.Setup(a => a.Balance).Returns(1000);
            mockAccount.SetupProperty(a => a.Balance);
            mockAccount.Setup(a => a.Pin).Returns("1234");
            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(100);
            mockAccount.Setup(a => a.Policy).Returns(mockPolicy.Object);

            double withdrawAmount = 500;

            // Act
            _accountManager.Withdraw(mockAccount.Object, withdrawAmount, "1234");

            // Assert
            Assert.AreEqual(500, mockAccount.Object.Balance);
            _mockAccountDAO.Verify(dao => dao.UpdateAccount(It.IsAny<IAccount>()), Times.Once);
            _mockTransactionDAO.Verify(dao => dao.LogTransaction(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPinException))]
        public void Withdraw_InvalidPin_ThrowsException()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(true);
            mockAccount.Setup(a => a.Pin).Returns("1234");

            // Act
            _accountManager.Withdraw(mockAccount.Object, 500, "4321");

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void TransferFunds_ValidInput_UpdatesBalances()
        {
            // Arrange
            var mockFromAccount = new Mock<IAccount>();
            mockFromAccount.Setup(a => a.Active).Returns(true);
            mockFromAccount.Setup(a => a.Balance).Returns(1000);
            mockFromAccount.SetupProperty(a => a.Balance);
            mockFromAccount.Setup(a => a.Pin).Returns("1234");
            mockFromAccount.Setup(a => a.PrivilegeType).Returns(PrivilegeType.REGULAR);

            var mockToAccount = new Mock<IAccount>();
            mockToAccount.Setup(a => a.Active).Returns(true);
            mockToAccount.Setup(a => a.Balance).Returns(500);
            mockToAccount.SetupProperty(a => a.Balance);

            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(100);
            mockFromAccount.Setup(a => a.Policy).Returns(mockPolicy.Object);

            var transfer = new Transfer
            {
                FromAcc = mockFromAccount.Object,
                ToAcc = mockToAccount.Object,
                Amount = 300,
                Pin = "1234"
            };

            _mockPrivilegeManager.Setup(pm => pm.GetDailyLimit(It.IsAny<PrivilegeType>())).Returns(1000);
            _mockTransactionDAO.Setup(dao => dao.GetDailyTransferAmount(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(0);

            // Act
            _accountManager.TransferFunds(transfer);

            // Assert
            Assert.AreEqual(700, mockFromAccount.Object.Balance);
            Assert.AreEqual(800, mockToAccount.Object.Balance);
            _mockAccountDAO.Verify(dao => dao.UpdateAccount(It.IsAny<IAccount>()), Times.Exactly(2));
            _mockTransactionDAO.Verify(dao => dao.LogTransaction(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(DailyLimitExceededException))]
        public void TransferFunds_ExceedsDailyLimit_ThrowsException()
        {
            // Arrange
            var mockFromAccount = new Mock<IAccount>();
            mockFromAccount.Setup(a => a.Active).Returns(true);
            mockFromAccount.Setup(a => a.Balance).Returns(2000);
            mockFromAccount.Setup(a => a.Pin).Returns("1234");
            mockFromAccount.Setup(a => a.PrivilegeType).Returns(PrivilegeType.REGULAR);

            var mockToAccount = new Mock<IAccount>();
            mockToAccount.Setup(a => a.Active).Returns(true);

            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(100);
            mockFromAccount.Setup(a => a.Policy).Returns(mockPolicy.Object);

            var transfer = new Transfer
            {
                FromAcc = mockFromAccount.Object,
                ToAcc = mockToAccount.Object,
                Amount = 1500,
                Pin = "1234"
            };

            _mockPrivilegeManager.Setup(pm => pm.GetDailyLimit(It.IsAny<PrivilegeType>())).Returns(1000);
            _mockTransactionDAO.Setup(dao => dao.GetDailyTransferAmount(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(0);

            // Act
            _accountManager.TransferFunds(transfer);

            // Assert is handled by ExpectedException
        }
    }
}