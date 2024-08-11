using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.DataAccess;
using System;
using System.Configuration;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class AccountManagerTests
    {
        private Mock<IAccountDAO> _mockAccountDAO;
        private Mock<ITransactionDAO> _mockTransactionDAO;
        private Mock<IExternalTransferDAO> _mockExternalTransferDAO;
        private Mock<AccountPrivilegeManager> _mockPrivilegeManager;
        private Mock<IPolicyFactory> _mockPolicyFactory;
        private AccountManager _accountManager;
        private ExternalBankServiceFactory _externalBankServiceFactory;

        [TestInitialize]
        public void Setup()
        {
            _mockAccountDAO = new Mock<IAccountDAO>();
            _mockTransactionDAO = new Mock<ITransactionDAO>();
            _mockExternalTransferDAO = new Mock<IExternalTransferDAO>();
            _mockPrivilegeManager = new Mock<AccountPrivilegeManager>();
            _mockPolicyFactory = new Mock<IPolicyFactory>();

            // Mock the configuration for ExternalBankServiceFactory
            var mockConfiguration = new Mock<Configuration>();
            var mockAppSettings = new Mock<AppSettingsSection>();
            mockAppSettings.Setup(m => m.Settings).Returns(new KeyValueConfigurationCollection());
            mockConfiguration.Setup(c => c.AppSettings).Returns(mockAppSettings.Object);

            // Initialize the ExternalBankServiceFactory
            //ExternalBankServiceFactory.Initialize(mockConfiguration.Object);
            _externalBankServiceFactory = ExternalBankServiceFactory.Instance;

            _accountManager = new AccountManager(
                _mockPrivilegeManager.Object,
                _mockAccountDAO.Object,
                _mockTransactionDAO.Object,
                _mockExternalTransferDAO.Object,
                _externalBankServiceFactory,
                _mockPolicyFactory.Object
            );
        }

        [TestMethod]
        public void CreateAccount_ValidInput_CreatesAccount()
        {
            // Arrange
            string name = "John Doe";
            string pin = "1234";
            double balance = 1000;
            PrivilegeType privilegeType = PrivilegeType.REGULAR;
            AccountType accountType = AccountType.SAVINGS;

            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(500);
            _mockPolicyFactory.Setup(f => f.CreatePolicy(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPolicy.Object);

            // Act
            IAccount createdAccount = _accountManager.CreateAccount(name, pin, balance, privilegeType, accountType);

            // Assert
            Assert.IsNotNull(createdAccount);
            Assert.AreEqual(name, createdAccount.Name);
            Assert.AreEqual(pin, createdAccount.Pin);
            Assert.AreEqual(balance, createdAccount.Balance);
            Assert.AreEqual(privilegeType, createdAccount.PrivilegeType);
            Assert.AreEqual(accountType, createdAccount.GetAccType());
            Assert.IsTrue(createdAccount.Active);

            _mockAccountDAO.Verify(dao => dao.CreateAccount(It.IsAny<IAccount>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(MinBalanceNeedsToBeMaintainedException))]
        public void CreateAccount_InsufficientBalance_ThrowsException()
        {
            // Arrange
            string name = "John Doe";
            string pin = "1234";
            double balance = 100;
            PrivilegeType privilegeType = PrivilegeType.REGULAR;
            AccountType accountType = AccountType.SAVINGS;

            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(500);
            _mockPolicyFactory.Setup(f => f.CreatePolicy(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPolicy.Object);

            // Act
            _accountManager.CreateAccount(name, pin, balance, privilegeType, accountType);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public void Deposit_ValidInput_UpdatesBalance()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(true);
            mockAccount.Setup(a => a.Balance).Returns(1000);
            mockAccount.Setup(a => a.AccNo).Returns("SAV1001");

            double depositAmount = 500;

            // Act
            _accountManager.Deposit(mockAccount.Object, depositAmount);

            // Assert
            mockAccount.VerifySet(a => a.Balance = 1500);
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

            double depositAmount = 500;

            // Act
            _accountManager.Deposit(mockAccount.Object, depositAmount);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public void Withdraw_ValidInput_UpdatesBalance()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.Active).Returns(true);
            mockAccount.Setup(a => a.Balance).Returns(1000);
            mockAccount.Setup(a => a.Pin).Returns("1234");
            mockAccount.Setup(a => a.AccNo).Returns("SAV1001");

            var mockPolicy = new Mock<IPolicy>();
            mockPolicy.Setup(p => p.GetMinBalance()).Returns(100);
            mockAccount.Setup(a => a.Policy).Returns(mockPolicy.Object);

            double withdrawAmount = 500;
            string pin = "1234";

            // Act
            _accountManager.Withdraw(mockAccount.Object, withdrawAmount, pin);

            // Assert
            mockAccount.VerifySet(a => a.Balance = 500);
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
            mockAccount.Setup(a => a.Policy).Returns(Mock.Of<IPolicy>());

            double withdrawAmount = 500;
            string invalidPin = "5678";

            // Act
            _accountManager.Withdraw(mockAccount.Object, withdrawAmount, invalidPin);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public void TransferFunds_ValidInput_UpdatesBalances()
        {
            // Arrange
            var mockFromAccount = new Mock<IAccount>();
            mockFromAccount.Setup(a => a.Active).Returns(true);
            mockFromAccount.Setup(a => a.Balance).Returns(1000);
            mockFromAccount.Setup(a => a.Pin).Returns("1234");
            mockFromAccount.Setup(a => a.AccNo).Returns("SAV1001");
            mockFromAccount.Setup(a => a.PrivilegeType).Returns(PrivilegeType.REGULAR);

            var mockToAccount = new Mock<IAccount>();
            mockToAccount.Setup(a => a.Active).Returns(true);
            mockToAccount.Setup(a => a.Balance).Returns(500);
            mockToAccount.Setup(a => a.AccNo).Returns("SAV1002");

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
            mockFromAccount.VerifySet(a => a.Balance = 700);
            mockToAccount.VerifySet(a => a.Balance = 800);
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
            mockFromAccount.Setup(a => a.AccNo).Returns("SAV1001");
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

            // Assert is handled by ExpectedException attribute
        }
    }
}