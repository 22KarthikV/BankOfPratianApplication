using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using Moq;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class TransactionServiceTests
    {
        private Mock<IAccountManager> _mockAccountManager;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Setup()
        {
            _mockAccountManager = new Mock<IAccountManager>();
            _transactionService = new TransactionService(_mockAccountManager.Object);
        }

        [TestMethod]
        public void ProcessDeposit_ValidDeposit_Succeeds()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.SetupProperty(a => a.Balance, 1000);
            mockAccount.Setup(a => a.AccNo).Returns("123");
            _mockAccountManager.Setup(m => m.GetAccount("123")).Returns(mockAccount.Object);
            _mockAccountManager.Setup(m => m.Deposit(mockAccount.Object, 500)).Callback(() => mockAccount.Object.Balance += 500);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                _transactionService.ProcessDeposit("123", 500);
                string result = sw.ToString().Trim();

                // Assert
                Assert.IsTrue(result.Contains("Deposit successful"));
                Assert.IsTrue(result.Contains("1,500.00"));
            }
        }

        [TestMethod]
        public void ProcessWithdrawal_ValidWithdrawal_Succeeds()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.SetupProperty(a => a.Balance, 1000);
            mockAccount.Setup(a => a.AccNo).Returns("123");
            _mockAccountManager.Setup(m => m.GetAccount("123")).Returns(mockAccount.Object);
            _mockAccountManager.Setup(m => m.Withdraw(mockAccount.Object, 500, "1234")).Callback(() => mockAccount.Object.Balance -= 500);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                _transactionService.ProcessWithdrawal("123", 500, "1234");
                string result = sw.ToString().Trim();

                // Assert
                Assert.IsTrue(result.Contains("Withdrawal successful"));
                Assert.IsTrue(result.Contains("500.00"));
            }
        }

        [TestMethod]
        public void ProcessWithdrawal_InsufficientBalance_Fails()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.SetupProperty(a => a.Balance, 100);
            mockAccount.Setup(a => a.AccNo).Returns("123");
            _mockAccountManager.Setup(m => m.GetAccount("123")).Returns(mockAccount.Object);
            _mockAccountManager.Setup(m => m.Withdraw(mockAccount.Object, 500, "1234")).Throws(new InsufficientBalanceException("Insufficient balance"));

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                _transactionService.ProcessWithdrawal("123", 500, "1234");
                string result = sw.ToString().Trim();

                // Assert
                Assert.IsTrue(result.Contains("Error: Insufficient balance"));
            }
        }

        [TestMethod]
        public void ProcessTransfer_ValidTransfer_Succeeds()
        {
            // Arrange
            var mockFromAccount = new Mock<IAccount>();
            mockFromAccount.SetupProperty(a => a.Balance, 1000);
            mockFromAccount.Setup(a => a.AccNo).Returns("123");

            var mockToAccount = new Mock<IAccount>();
            mockToAccount.SetupProperty(a => a.Balance, 500);
            mockToAccount.Setup(a => a.AccNo).Returns("456");

            _mockAccountManager.Setup(m => m.GetAccount("123")).Returns(mockFromAccount.Object);
            _mockAccountManager.Setup(m => m.GetAccount("456")).Returns(mockToAccount.Object);
            _mockAccountManager.Setup(m => m.TransferFunds(It.IsAny<Transfer>())).Callback(() =>
            {
                mockFromAccount.Object.Balance -= 500;
                mockToAccount.Object.Balance += 500;
            });

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                _transactionService.ProcessTransfer("123", "456", 500, "1234");
                string result = sw.ToString().Trim();

                // Assert
                Assert.IsTrue(result.Contains("Transfer successful"));
                Assert.IsTrue(result.Contains("500.00"));
            }
        }

        [TestMethod]
        public void ProcessTransfer_DailyLimitExceeded_Fails()
        {
            // Arrange
            var mockFromAccount = new Mock<IAccount>();
            mockFromAccount.SetupProperty(a => a.Balance, 10000);
            mockFromAccount.Setup(a => a.AccNo).Returns("123");

            var mockToAccount = new Mock<IAccount>();
            mockToAccount.SetupProperty(a => a.Balance, 500);
            mockToAccount.Setup(a => a.AccNo).Returns("456");

            _mockAccountManager.Setup(m => m.GetAccount("123")).Returns(mockFromAccount.Object);
            _mockAccountManager.Setup(m => m.GetAccount("456")).Returns(mockToAccount.Object);
            _mockAccountManager.Setup(m => m.TransferFunds(It.IsAny<Transfer>())).Throws(new DailyLimitExceededException("Daily transfer limit exceeded"));

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                _transactionService.ProcessTransfer("123", "456", 5000, "1234");
                string result = sw.ToString().Trim();

                // Assert
                Assert.IsTrue(result.Contains("Error: Daily transfer limit exceeded"));
            }
        }
    }
}