using System;
using System.Linq;
using System.Threading;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.DataAccess;
using NLog;

namespace BankOfPratian.Business
{
    public class ExternalTransferService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IExternalTransferDAO _externalTransferDAO;
        private readonly ExternalBankServiceFactory _externalBankServiceFactory;
        private readonly Thread _workerThread;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // Delegates for account operations
        private readonly Func<string, IAccount> _getAccount;
        private readonly Action<IAccount, double, string> _withdraw;
        private readonly Func<PrivilegeType, double> _getDailyLimit;
        private readonly Func<string, double> _getDailyTransferAmount;

        public ExternalTransferService(
            IExternalTransferDAO externalTransferDAO,
            ExternalBankServiceFactory externalBankServiceFactory,
            Func<string, IAccount> getAccount,
            Action<IAccount, double, string> withdraw,
            Func<PrivilegeType, double> getDailyLimit,
            Func<string, double> getDailyTransferAmount)
        {
            _externalTransferDAO = externalTransferDAO;
            _externalBankServiceFactory = externalBankServiceFactory;
            _getAccount = getAccount;
            _withdraw = withdraw;
            _getDailyLimit = getDailyLimit;
            _getDailyTransferAmount = getDailyTransferAmount;
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(Run);
        }

        public void Start()
        {
            _workerThread.Start();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _workerThread.Join();
        }

        private void Run()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    ProcessOpenExternalTransfers();
                    Thread.Sleep(TimeSpan.FromMinutes(5)); // Check every 5 minutes
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error processing external transfers");
                }
            }
        }

        private void ProcessOpenExternalTransfers()
        {
            var openTransfers = _externalTransferDAO.GetOpenExternalTransfers();
            foreach (var transfer in openTransfers)
            {
                ProcessExternalTransfer(transfer);
            }
        }

      

        private void ProcessExternalTransfer(ExternalTransfer transfer)
        {
            Logger.Info($"Processing external transfer: {transfer.TransID}");
            try
            {
                var externalBankService = _externalBankServiceFactory.GetExternalBankService(transfer.ToExternalAcc.Substring(0, 4));
                Logger.Debug($"External bank service retrieved for bank code: {transfer.ToExternalAcc.Substring(0, 4)}");

                bool depositResult = externalBankService.Deposit(transfer.ToExternalAcc, transfer.Amount);
                Logger.Debug($"Deposit result: {depositResult}");

                if (depositResult)
                {
                    transfer.Status = TransactionStatus.CLOSED;
                    var fromAccount = _getAccount(transfer.FromAccountNo);
                    Logger.Debug($"Retrieved source account: {fromAccount.AccNo}");

                    _withdraw(fromAccount, transfer.Amount, transfer.FromAccPin);
                    Logger.Debug($"Withdrawal completed from account: {fromAccount.AccNo}");

                    _externalTransferDAO.UpdateExternalTransfer(transfer);
                    Logger.Info($"External transfer completed: {transfer.TransID}");
                }
                else
                {
                    transfer.Status = TransactionStatus.FAILED;
                    _externalTransferDAO.UpdateExternalTransfer(transfer);
                    Logger.Warn($"External transfer failed: {transfer.TransID}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error processing external transfer: {transfer.TransID}");
                transfer.Status = TransactionStatus.FAILED;
                _externalTransferDAO.UpdateExternalTransfer(transfer);
            }
        }


        public void InitiateExternalTransfer(ExternalTransfer transfer)
        {
            try
            {
                if (transfer == null)
                {
                    throw new ArgumentNullException(nameof(transfer), "Transfer object cannot be null");
                }

                if (string.IsNullOrEmpty(transfer.FromAccountNo))
                {
                    throw new ArgumentException("FromAccountNo cannot be null or empty", nameof(transfer));
                }

                var fromAccount = _getAccount(transfer.FromAccountNo);
                // The policy should now be guaranteed to exist, or an exception would have been thrown

                if (!fromAccount.Active)
                {
                    throw new InactiveAccountException($"Account {transfer.FromAccountNo} is inactive");
                }

                if (fromAccount.Pin != transfer.FromAccPin)
                {
                    throw new InvalidPinException("Invalid PIN");
                }

                double minBalance = fromAccount.Policy.GetMinBalance();
                if (fromAccount.Balance - transfer.Amount < minBalance)
                {
                    throw new InsufficientBalanceException($"Insufficient balance. Minimum balance requirement: {minBalance}");
                }

                double dailyLimit = _getDailyLimit(fromAccount.PrivilegeType);
                double dailyTransferAmount = _getDailyTransferAmount(transfer.FromAccountNo);
                if (dailyTransferAmount + transfer.Amount > dailyLimit)
                {
                    throw new DailyLimitExceededException($"Daily transfer limit of {dailyLimit} exceeded");
                }

                //transfer.TransID = IDGenerator.GenerateTransactionID();
                transfer.TranDate = DateTime.Now;
                transfer.Status = TransactionStatus.OPEN;
                transfer.FromAccount = fromAccount; // Ensure FromAccount is set

                _externalTransferDAO.CreateExternalTransfer(transfer);
                Logger.Info($"External transfer initiated: {transfer.TransID}");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, $"Policy creation failed for account {transfer?.FromAccountNo}");
                throw new ExternalTransferException("Unable to process transfer due to account policy issues", ex);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error initiating external transfer for account {transfer?.FromAccountNo}");
                throw new ExternalTransferException("Error initiating external transfer", ex);
            }
        }
    }

    
}