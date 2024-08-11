using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using Moq;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class TransactionLogTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Clear the transaction log before each test
            typeof(TransactionLog)
                .GetField("Log", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, new System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<TransactionType, List<Transaction>>>());
        }

        [TestMethod]
        public void LogTransaction_ValidTransaction_AddsToLog()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.AccNo).Returns("123");
            var transaction = new Transaction { FromAccount = mockAccount.Object, Amount = 100, TranDate = DateTime.Now, Type = TransactionType.DEPOSIT };

            // Act
            TransactionLog.LogTransaction("123", TransactionType.DEPOSIT, transaction);

            // Assert
            var transactions = TransactionLog.GetTransactions("123");
            Assert.AreEqual(1, transactions[TransactionType.DEPOSIT].Count);
            Assert.AreEqual(100, transactions[TransactionType.DEPOSIT][0].Amount);
        }

        // ... [Other test methods remain unchanged]

        [TestMethod]
        public void GetTransactions_ValidAccount_ReturnsCorrectTransactions()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.AccNo).Returns("123");
            var deposit = new Transaction { FromAccount = mockAccount.Object, Amount = 100, TranDate = DateTime.Now, Type = TransactionType.DEPOSIT };
            var withdrawal = new Transaction { FromAccount = mockAccount.Object, Amount = 50, TranDate = DateTime.Now, Type = TransactionType.WITHDRAW };
            TransactionLog.LogTransaction("123", TransactionType.DEPOSIT, deposit);
            TransactionLog.LogTransaction("123", TransactionType.WITHDRAW, withdrawal);

            // Act
            var transactions = TransactionLog.GetTransactions("123");

            // Assert
            Assert.AreEqual(2, transactions.Count);
            Assert.AreEqual(1, transactions[TransactionType.DEPOSIT].Count);
            Assert.AreEqual(1, transactions[TransactionType.WITHDRAW].Count);
        }

        [TestMethod]
        public void LogTransaction_MultipleTransactionTypes_AddsToCorrectLogs()
        {
            // Arrange
            var mockAccount = new Mock<IAccount>();
            mockAccount.Setup(a => a.AccNo).Returns("123");
            var deposit = new Transaction { FromAccount = mockAccount.Object, Amount = 100, TranDate = DateTime.Now, Type = TransactionType.DEPOSIT };
            var withdrawal = new Transaction { FromAccount = mockAccount.Object, Amount = 50, TranDate = DateTime.Now, Type = TransactionType.WITHDRAW };
            var transfer = new Transaction { FromAccount = mockAccount.Object, Amount = 75, TranDate = DateTime.Now, Type = TransactionType.TRANSFER };

            // Act
            TransactionLog.LogTransaction("123", TransactionType.DEPOSIT, deposit);
            TransactionLog.LogTransaction("123", TransactionType.WITHDRAW, withdrawal);
            TransactionLog.LogTransaction("123", TransactionType.TRANSFER, transfer);

            // Assert
            var transactions = TransactionLog.GetTransactions("123");
            Assert.AreEqual(3, transactions.Count);
            Assert.AreEqual(1, transactions[TransactionType.DEPOSIT].Count);
            Assert.AreEqual(1, transactions[TransactionType.WITHDRAW].Count);
            Assert.AreEqual(1, transactions[TransactionType.TRANSFER].Count);
        }
    }
}