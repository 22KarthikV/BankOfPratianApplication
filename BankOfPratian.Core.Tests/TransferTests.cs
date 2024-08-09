using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Core;

namespace BankOfPratian.Core.Tests
{
    [TestClass]
    public class TransferTests
    {
        [TestMethod]
        public void Transfer_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var fromAccount = new SavingsAccount();
            var toAccount = new CurrentAccount();

            // Act
            var transfer = new Transfer
            {
                FromAcc = fromAccount,
                ToAcc = toAccount,
                Amount = 500,
                Pin = "1234"
            };

            // Assert
            Assert.AreEqual(fromAccount, transfer.FromAcc);
            Assert.AreEqual(toAccount, transfer.ToAcc);
            Assert.AreEqual(500, transfer.Amount);
            Assert.AreEqual("1234", transfer.Pin);
        }
    }
}