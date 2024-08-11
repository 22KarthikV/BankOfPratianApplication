using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankOfPratian.Business;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.DataAccess;
using System;
using System.Collections.Generic;

namespace BankOfPratian.Tests
{
    [TestClass]
    public class ExternalTransferServiceTests
    {
        private Mock<IExternalTransferDAO> _mockExternalTransferDAO;
        private Mock<ExternalBankServiceFactory> _mockExternalBankServiceFactory;
        private Mock<IExternalBankService> _mockExternalBankService;
        private ExternalTransferService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockExternalTransferDAO = new Mock<IExternalTransferDAO>();
            _mockExternalBankServiceFactory = new Mock<ExternalBankServiceFactory>();
            _mockExternalBankService = new Mock<IExternalBankService>();

            _mockExternalBankServiceFactory.Setup(f => f.GetExternalBankService(It.IsAny<string>()))
                .Returns(_mockExternalBankService.Object);

            _service = new ExternalTransferService(
                _mockExternalTransferDAO.Object,
                _mockExternalBankServiceFactory.Object,
                (accNo) => new SavingsAccount { Balance = 1000 },
                (acc, amount, pin) => { acc.Balance -= amount; },
                (privilegeType) => 1000,
                (accNo) => 0
            );
        }

        [TestMethod]
        public void InitiateExternalTransfer_ValidTransfer_Success()
        {
            // Arrange
            var transfer = new ExternalTransfer
            {
                FromAccountNo = "SAV1001",
                ToExternalAcc = "EXT1001",
                Amount = 500,
                FromAccPin = "1234"
            };

            // Act
            _service.InitiateExternalTransfer(transfer);

            // Assert
            _mockExternalTransferDAO.Verify(dao => dao.CreateExternalTransfer(It.IsAny<ExternalTransfer>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InsufficientBalanceException))]
        public void InitiateExternalTransfer_InsufficientBalance_ThrowsException()
        {
            // Arrange
            var transfer = new ExternalTransfer
            {
                FromAccountNo = "SAV1001",
                ToExternalAcc = "EXT1001",
                Amount = 1500,  // More than the balance
                FromAccPin = "1234"
            };

            // Act & Assert
            _service.InitiateExternalTransfer(transfer);
        }

        [TestMethod]
        public void ProcessExternalTransfer_SuccessfulDeposit_UpdatesStatus()
        {
            // Arrange
            var transfer = new ExternalTransfer
            {
                TransID = 1,
                FromAccountNo = "SAV1001",
                ToExternalAcc = "EXT1001",
                Amount = 500,
                Status = TransactionStatus.OPEN
            };

            _mockExternalBankService.Setup(s => s.Deposit(It.IsAny<string>(), It.IsAny<double>())).Returns(true);

            // Act
            _service.ProcessExternalTransfer(transfer);

            // Assert
            Assert.AreEqual(TransactionStatus.CLOSED, transfer.Status);
            _mockExternalTransferDAO.Verify(dao => dao.UpdateExternalTransfer(It.IsAny<ExternalTransfer>()), Times.Once);
        }
    }
}