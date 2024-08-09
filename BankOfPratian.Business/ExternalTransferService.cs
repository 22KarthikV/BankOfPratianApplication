using System;
using System.Linq;
using System.Threading;
using BankOfPratian.Core;
using NLog;

namespace BankOfPratian.Business
{
    public class ExternalTransferService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AccountManager _accountManager;
        private readonly Thread _workerThread;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ExternalTransferService(AccountManager accountManager)
        {
            _accountManager = accountManager;
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
            var allTransactions = TransactionLog.GetTransactions();
            foreach (var accountTransactions in allTransactions.Values)
            {
                if (accountTransactions.TryGetValue(TransactionType.EXTERNALTRANSFER, out var externalTransfers))
                {
                    foreach (var transaction in externalTransfers.Cast<ExternalTransfer>().Where(t => t.Status == TransactionStatus.OPEN))
                    {
                        ProcessExternalTransfer(transaction);
                    }
                }
            }
        }

        private void ProcessExternalTransfer(ExternalTransfer transfer)
        {
            try
            {
                var externalBankService = ExternalBankServiceFactory.Instance.GetExternalBankService(transfer.ToExternalAcc.Substring(0, 4)); // Assuming first 4 digits are bank code
                if (externalBankService.Deposit(transfer.ToExternalAcc, transfer.Amount))
                {
                    transfer.Status = TransactionStatus.CLOSED;
                    _accountManager.Withdraw(transfer.FromAccount, transfer.Amount, transfer.FromAccPin);
                    Logger.Info($"External transfer completed: {transfer.TransID}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error processing external transfer: {transfer.TransID}");
            }
        }
    }
}