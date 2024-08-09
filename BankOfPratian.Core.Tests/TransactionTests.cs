using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Core;
using System;

namespace BankOfPratian.Core.Tests
{
    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void Transaction_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var account = new SavingsAccount();
            var transactionDate = DateTime.Now;

            // Act
            var transaction = new Transaction
            {
                TransID = 1,
                FromAccount = account,
                TranDate = transactionDate,
                Amount = 100,
                Status = TransactionStatus.OPEN,
                Type = TransactionType.DEPOSIT
            };

            // Assert
            Assert.AreEqual(1, transaction.TransID);
            Assert.AreEqual(account, transaction.FromAccount);
            Assert.AreEqual(transactionDate, transaction.TranDate);
            Assert.AreEqual(100, transaction.Amount);
            Assert.AreEqual(TransactionStatus.OPEN, transaction.Status);
            Assert.AreEqual(TransactionType.DEPOSIT, transaction.Type);
        }

        [TestMethod]
        public void ExternalTransfer_Constructor_SetsAdditionalPropertiesCorrectly()
        {
            var account = new CurrentAccount();
            var transactionDate = DateTime.Now;
            var externalTransfer = new ExternalTransfer
            {
                TransID = 2,
                FromAccount = account,
                TranDate = transactionDate,
                Amount = 200,
                Status = TransactionStatus.OPEN,
                ToExternalAcc = "EXT123",
                FromAccPin = "1234"
            };

            Assert.AreEqual(2, externalTransfer.TransID);
            Assert.AreEqual(account, externalTransfer.FromAccount);
            Assert.AreEqual(transactionDate, externalTransfer.TranDate);
            Assert.AreEqual(200, externalTransfer.Amount);
            Assert.AreEqual(TransactionStatus.OPEN, externalTransfer.Status);
            Assert.AreEqual("EXT123", externalTransfer.ToExternalAcc);
            Assert.AreEqual("1234", externalTransfer.FromAccPin);
        }

        [TestMethod]
        public void Transfer_Constructor_SetsPropertiesCorrectly()
        {
            var fromAccount = new SavingsAccount();
            var toAccount = new CurrentAccount();
            var transfer = new Transfer
            {
                FromAcc = fromAccount,
                ToAcc = toAccount,
                Amount = 300,
                Pin = "5678"
            };

            Assert.AreEqual(fromAccount, transfer.FromAcc);
            Assert.AreEqual(toAccount, transfer.ToAcc);
            Assert.AreEqual(300, transfer.Amount);
            Assert.AreEqual("5678", transfer.Pin);
        }
    }
}