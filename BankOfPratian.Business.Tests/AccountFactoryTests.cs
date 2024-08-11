using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.Business;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class AccountFactoryTests
    {
        [TestMethod]
        public void CreateAccount_ShouldReturnSavingsAccount_WhenSavingsAccountTypeIsPassed()
        {
            // Arrange
            var accountType = AccountType.SAVINGS;

            // Act
            var account = AccountFactory.CreateAccount(accountType);

            // Assert
            Assert.IsInstanceOfType(account, typeof(SavingsAccount));
        }

        [TestMethod]
        public void CreateAccount_ShouldReturnCurrentAccount_WhenCurrentAccountTypeIsPassed()
        {
            // Arrange
            var accountType = AccountType.CURRENT;

            // Act
            var account = AccountFactory.CreateAccount(accountType);

            // Assert
            Assert.IsInstanceOfType(account, typeof(CurrentAccount));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidAccountTypeException))]
        public void CreateAccount_ShouldThrowInvalidAccountTypeException_WhenInvalidAccountTypeIsPassed()
        {
            // Arrange
            var invalidAccountType = (AccountType)999; // Assuming 999 is not a valid AccountType

            // Act
            AccountFactory.CreateAccount(invalidAccountType);

            // Assert is handled by ExpectedException
        }
    }
}
