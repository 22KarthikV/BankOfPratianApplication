using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;

namespace BankOfPratian.Business.Tests
{
    [TestClass]
    public class AccountFactoryTests
    {
        [TestMethod]
        public void CreateAccount_SavingsAccount_ReturnsSavingsAccount()
        {
            // Act
            var account = AccountFactory.CreateAccount(AccountType.SAVINGS);

            // Assert
            Assert.IsNotNull(account);
            Assert.AreEqual(AccountType.SAVINGS, account.GetAccType());
        }

        [TestMethod]
        public void CreateAccount_CurrentAccount_ReturnsCurrentAccount()
        {
            // Act
            var account = AccountFactory.CreateAccount(AccountType.CURRENT);

            // Assert
            Assert.IsNotNull(account);
            Assert.AreEqual(AccountType.CURRENT, account.GetAccType());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidAccountTypeException))]
        public void CreateAccount_InvalidAccountType_ThrowsException()
        {
            // Act
            AccountFactory.CreateAccount((AccountType)999);

            // Assert is handled by ExpectedException
        }
    }
}