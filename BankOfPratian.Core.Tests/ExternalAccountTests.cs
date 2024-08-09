using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankOfPratian.Core;

namespace BankOfPratian.Core.Tests
{
    [TestClass]
    public class ExternalAccountTests
    {
        [TestMethod]
        public void ExternalAccount_Constructor_SetsPropertiesCorrectly()
        {
            var externalAccount = new ExternalAccount
            {
                AccNo = "EXT456",
                BankCode = "EXTBANK",
                BankName = "External Bank"
            };

            Assert.AreEqual("EXT456", externalAccount.AccNo);
            Assert.AreEqual("EXTBANK", externalAccount.BankCode);
            Assert.AreEqual("External Bank", externalAccount.BankName);
        }
    }
}