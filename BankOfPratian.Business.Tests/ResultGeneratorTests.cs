using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.DataAccess;
using Moq;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class ResultGeneratorTests
    {
        private Mock<IAccountDAO> _mockAccountDAO;
        private Mock<ITransactionDAO> _mockTransactionDAO;
        private Mock<IExternalTransferDAO> _mockExternalTransferDAO;

        [TestInitialize]
        public void Setup()
        {
            _mockAccountDAO = new Mock<IAccountDAO>();
            _mockTransactionDAO = new Mock<ITransactionDAO>();
            _mockExternalTransferDAO = new Mock<IExternalTransferDAO>();

            // Inject mocks into ResultGenerator
            typeof(ResultGenerator)
                .GetField("_accountDAO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, _mockAccountDAO.Object);

            typeof(ResultGenerator)
                .GetField("_transactionDAO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, _mockTransactionDAO.Object);

            typeof(ResultGenerator)
                .GetField("_externalTransferDAO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, _mockExternalTransferDAO.Object);
        }

        [TestMethod]
        public void GetTotalNoOfAccounts_ReturnsCorrectCount()
        {
            // Arrange
            _mockAccountDAO.Setup(dao => dao.GetTotalAccountCount()).Returns(10);

            // Act
            int result = ResultGenerator.GetTotalNoOfAccounts();

            // Assert
            Assert.AreEqual(10, result);
        }

        /*[TestMethod]
        public void DisplayNoOfAccTypeWise_DisplaysCorrectCounts()
        {
            // Arrange
            var accountTypeCounts = new Dictionary<AccountType, int>
            {
                { AccountType.Savings, 5 },
                { AccountType.Current, 3 }
            };
            _mockAccountDAO.Setup(dao => dao.GetAccountTypeCount()).Returns(accountTypeCounts);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                ResultGenerator.DisplayNoOfAccTypeWise();
                string result = sw.ToString().Trim();

                // Assert
                StringAssert.Contains(result, "Savings      |               5");
                StringAssert.Contains(result, "Current      |               3");
            }
        }*/

        [TestMethod]
        public void DispTotalWorthOfBank_DisplaysCorrectTotal()
        {
            // Arrange
            _mockAccountDAO.Setup(dao => dao.GetTotalBankWorth()).Returns(1000000.50);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                ResultGenerator.DispTotalWorthOfBank();
                string result = sw.ToString().Trim();

                // Assert
                StringAssert.Contains(result, "Total balance available: $1,000,000.50");
            }
        }

        [TestMethod]
        public void DisplayAllTransfers_DisplaysCorrectTransfers()
        {
            // Arrange
            var mockFromAccount = new Mock<IAccount>();
            mockFromAccount.Setup(a => a.AccNo).Returns("123");

            var internalTransfers = new List<Transaction>
            {
                new Transaction { FromAccount = mockFromAccount.Object, TranDate = DateTime.Now, Amount = 100, Status = TransactionStatus.CLOSED, Type = TransactionType.TRANSFER }
            };
            var externalTransfers = new List<ExternalTransfer>
            {
                new ExternalTransfer { FromAccountNo = "456", ToExternalAcc = "789", TranDate = DateTime.Now, Amount = 200, Status = TransactionStatus.OPEN }
            };

            _mockTransactionDAO.Setup(dao => dao.GetAllTransactions()).Returns(internalTransfers);
            _mockExternalTransferDAO.Setup(dao => dao.GetAllExternalTransfers()).Returns(externalTransfers);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                ResultGenerator.DisplayAllTransfers();
                string result = sw.ToString().Trim();

                // Assert
                StringAssert.Contains(result, "INTERNAL | 123        | N/A        |");
                StringAssert.Contains(result, "EXTERNAL | 456        | 789        |");
            }
        }

        [TestMethod]
        public void DisplayAllTransactionsForToday_DisplaysCorrectTransactions()
        {
            // Arrange
            var today = DateTime.Today;
            var mockAccount1 = new Mock<IAccount>();
            mockAccount1.Setup(a => a.AccNo).Returns("123");
            var mockAccount2 = new Mock<IAccount>();
            mockAccount2.Setup(a => a.AccNo).Returns("456");

            var transactions = new List<Transaction>
            {
                new Transaction { FromAccount = mockAccount1.Object, TranDate = today, Amount = 100, Type = TransactionType.DEPOSIT },
                new Transaction { FromAccount = mockAccount2.Object, TranDate = today, Amount = 200, Type = TransactionType.WITHDRAW }
            };
            var externalTransfers = new List<ExternalTransfer>
            {
                new ExternalTransfer { FromAccountNo = "789", ToExternalAcc = "ICICI123", TranDate = today, Amount = 300 }
            };

            _mockTransactionDAO.Setup(dao => dao.GetAllTransactions()).Returns(transactions);
            _mockExternalTransferDAO.Setup(dao => dao.GetOpenExternalTransfers()).Returns(externalTransfers);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                ResultGenerator.DisplayAllTransactionsForToday();
                string result = sw.ToString().Trim();

                // Assert
                StringAssert.Contains(result, "DEPOSIT    | 123        | N/A        |");
                StringAssert.Contains(result, "WITHDRAW   | 456        | N/A        |");
                StringAssert.Contains(result, "EXTERNAL   | 789        | ICICI123   |");
            }
        }

        [TestMethod]
        public void DisplayAllTransactions_DisplaysCorrectTransactions()
        {
            // Arrange
            var mockAccount1 = new Mock<IAccount>();
            mockAccount1.Setup(a => a.AccNo).Returns("123");
            var mockAccount2 = new Mock<IAccount>();
            mockAccount2.Setup(a => a.AccNo).Returns("456");

            var transactions = new List<Transaction>
            {
                new Transaction { FromAccount = mockAccount1.Object, TranDate = DateTime.Now.AddDays(-1), Amount = 100, Type = TransactionType.DEPOSIT },
                new Transaction { FromAccount = mockAccount2.Object, TranDate = DateTime.Now, Amount = 200, Type = TransactionType.WITHDRAW }
            };

            _mockTransactionDAO.Setup(dao => dao.GetAllTransactions()).Returns(transactions);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                ResultGenerator.DisplayAllTransactions();
                string result = sw.ToString().Trim();

                // Assert
                StringAssert.Contains(result, "123        | DEPOSIT   |");
                StringAssert.Contains(result, "456        | WITHDRAW  |");
            }
        }

        [TestMethod]
        public void DisplayAllWithdrawals_DisplaysCorrectWithdrawals()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.AccNo).Returns("123");

            var transactions = new List<Transaction>
            {
                new Transaction { FromAccount = mockAccount.Object, TranDate = DateTime.Now, Amount = 200, Type = TransactionType.WITHDRAW }
            };

            _mockTransactionDAO.Setup(dao => dao.GetAllTransactions()).Returns(transactions);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                ResultGenerator.DisplayAllWithdrawals();
                string result = sw.ToString().Trim();

                // Assert
                StringAssert.Contains(result, "123        |");
                StringAssert.Contains(result, "200.00");
            }
        }

        [TestMethod]
        public void DisplayAllDeposits_DisplaysCorrectDeposits()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.AccNo).Returns("123");

            var transactions = new List<Transaction>
            {
                new Transaction { FromAccount = mockAccount.Object, TranDate = DateTime.Now, Amount = 300, Type = TransactionType.DEPOSIT }
            };

            _mockTransactionDAO.Setup(dao => dao.GetAllTransactions()).Returns(transactions);

            // Act
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                ResultGenerator.DisplayAllDeposits();
                string result = sw.ToString().Trim();

                // Assert
                StringAssert.Contains(result, "123        |");
                StringAssert.Contains(result, "300.00");
            }
        }
    }
}