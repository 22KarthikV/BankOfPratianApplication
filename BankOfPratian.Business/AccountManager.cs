using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.DataAccess;
using NLog;

namespace BankOfPratian.Business
{
    public class AccountManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AccountPrivilegeManager _privilegeManager;
        private readonly IAccountDAO _accountDAO;
        private readonly ITransactionDAO _transactionDAO;
        private readonly IPolicyFactory _policyFactory;
        private readonly ExternalTransferService _externalTransferService;

        public AccountManager(
            AccountPrivilegeManager privilegeManager,
            IAccountDAO accountDAO,
            ITransactionDAO transactionDAO,
            IExternalTransferDAO externalTransferDAO,
            ExternalBankServiceFactory externalBankServiceFactory,
            IPolicyFactory policyFactory)
        {
            _privilegeManager = privilegeManager;
            _accountDAO = accountDAO;
            _transactionDAO = transactionDAO;
            _policyFactory = policyFactory;

            _externalTransferService = new ExternalTransferService(
                externalTransferDAO,
                externalBankServiceFactory,
                GetAccount,
                Withdraw,
                GetDailyLimit,
                GetDailyTransferAmount
            );
        }


        public IAccount CreateAccount(string name, string pin, double balance, PrivilegeType privilegeType, AccountType accType)
        {
            try
            {
                Logger.Debug($"Creating account: Name={name}, PrivilegeType={privilegeType}, AccountType={accType}, Balance={balance}");

                var account = AccountFactory.CreateAccount(accType);
                Logger.Debug($"Account created with AccNo: {account.AccNo}");

                account.Name = name;
                account.Pin = pin;
                account.Balance = balance;
                account.PrivilegeType = privilegeType;
                account.DateOfOpening = DateTime.Now;
                account.Active = true;

                IPolicy policy = _policyFactory.CreatePolicy(accType.ToString(), privilegeType.ToString());
                account.Policy = policy;
                Logger.Debug($"Policy created: MinBalance={policy.GetMinBalance()}, RateOfInterest={policy.GetRateOfInterest()}");

                if (balance < policy.GetMinBalance())
                {
                    Logger.Warn($"Initial balance {balance} is less than minimum balance {policy.GetMinBalance()}");
                    throw new MinBalanceNeedsToBeMaintainedException($"Initial balance {balance} is less than minimum balance {policy.GetMinBalance()}");
                }

                if (account.Open())
                {
                    Logger.Debug("Account opened successfully");
                    _accountDAO.CreateAccount(account);
                    Logger.Info($"Account created and saved to database: {account.AccNo}");
                    return account;
                }

                Logger.Error("Unable to open the account");
                throw new UnableToOpenAccountException("Unable to open the account");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error creating account");
                throw;
            }
        }



        public void Deposit(IAccount toAccount, double amount)
        {
            try
            {
                if (!toAccount.Active)
                {
                    throw new InactiveAccountException($"Account {toAccount.AccNo} is inactive");
                }
                toAccount.Balance += amount;
                _accountDAO.UpdateAccount(toAccount);
                Logger.Info($"Deposit of {amount} to account {toAccount.AccNo}");
                LogTransaction(toAccount, TransactionType.DEPOSIT, amount);
            }
            catch (DatabaseOperationException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 515)
            {
                Logger.Error(ex, $"Error depositing to account {toAccount.AccNo}: Failed to log transaction due to null value in 'status' column");
                throw new DatabaseOperationException($"Failed to deposit to account {toAccount.AccNo} due to database constraints", ex);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error depositing to account {toAccount.AccNo}");
                throw;
            }
        }

        public void Withdraw(IAccount fromAccount, double amount, string pin)
        {
            try
            {
                if (!fromAccount.Active)
                {
                    throw new InactiveAccountException($"Account {fromAccount.AccNo} is inactive");
                }
                if (fromAccount.Pin != pin)
                {
                    throw new InvalidPinException("Invalid PIN");
                }
                if (fromAccount.Policy == null)
                {
                    IPolicy policy = _policyFactory.CreatePolicy(fromAccount.GetAccType().ToString(), fromAccount.PrivilegeType.ToString());
                    fromAccount.Policy = policy;
                }
                if (fromAccount.Balance - amount < fromAccount.Policy.GetMinBalance())
                {
                    throw new InsufficientBalanceException("Insufficient balance");
                }
                fromAccount.Balance -= amount;
                _accountDAO.UpdateAccount(fromAccount);
                Logger.Info($"Withdrawal of {amount} from account {fromAccount.AccNo}");
                LogTransaction(fromAccount, TransactionType.WITHDRAW, amount);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error withdrawing from account");
                throw;
            }
        }

        public void TransferFunds(Transfer transfer)
        {
            try
            {
                if (!transfer.FromAcc.Active || !transfer.ToAcc.Active)
                {
                    throw new InactiveAccountException("One or both accounts are inactive");
                }

                if (transfer.FromAcc.Pin != transfer.Pin)
                {
                    throw new InvalidPinException("Invalid PIN");
                }
                if (transfer.FromAcc.Policy == null)
                {
                    IPolicy policy = _policyFactory.CreatePolicy(transfer.FromAcc.GetAccType().ToString(), transfer.FromAcc.PrivilegeType.ToString());
                    transfer.FromAcc.Policy = policy;
                }

                if (transfer.FromAcc.Balance - transfer.Amount < transfer.FromAcc.Policy.GetMinBalance())
                {
                    throw new InsufficientBalanceException("Insufficient balance");
                }

                double dailyLimit = _privilegeManager.GetDailyLimit(transfer.FromAcc.PrivilegeType);
                double dailyTransferAmount = GetDailyTransferAmount(transfer.FromAcc.AccNo);

                if (dailyTransferAmount + transfer.Amount > dailyLimit)
                {
                    throw new DailyLimitExceededException($"Daily transfer limit of {dailyLimit} exceeded");
                }

                transfer.FromAcc.Balance -= transfer.Amount;
                transfer.ToAcc.Balance += transfer.Amount;

                _accountDAO.UpdateAccount(transfer.FromAcc);
                _accountDAO.UpdateAccount(transfer.ToAcc);

                Logger.Info($"Transfer of {transfer.Amount} from account {transfer.FromAcc.AccNo} to {transfer.ToAcc.AccNo}");
                LogTransaction(transfer.FromAcc, TransactionType.TRANSFER, transfer.Amount);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error transferring funds");
                throw;
            }
        }

        public double GetDailyTransferAmount(string accNo)
        {
            // Implement this method to retrieve the total transfer amount for the current day
            // You can use _transactionDAO to query the database for this information
            return _transactionDAO.GetDailyTransferAmount(accNo, DateTime.Today);
        }

        public void TransferFundsToExternal(ExternalTransfer transfer)
        {
            try
            {
                // Generate the TransID here
                transfer.TransID = IDGenerator.GenerateTransactionID(true);

                _externalTransferService.InitiateExternalTransfer(transfer);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error transferring funds to external account");
                throw;
            }
        }

        public double GetDailyLimit(PrivilegeType privilegeType)
        {
            return _privilegeManager.GetDailyLimit(privilegeType);
        }

        



        private void LogTransaction(IAccount account, TransactionType type, double amount)
        {
            var transaction = new Transaction
            {
                TransID = IDGenerator.GenerateTransactionID(),
                FromAccount = account,
                TranDate = DateTime.Now,
                Amount = amount,
                Type = type,
                Status = TransactionStatus.CLOSED
            };

            try
            {
                _transactionDAO.LogTransaction(transaction);
                Logger.Info($"Transaction logged: Type={type}, Account={account.AccNo}, Amount={amount}");
            }
            catch (DatabaseOperationException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 515)
            {
                Logger.Error(ex, $"Failed to log transaction due to null value in 'status' column: Type={type}, Account={account.AccNo}, Amount={amount}");
                throw new DatabaseOperationException("Failed to log transaction due to database constraints", ex);
            }
            catch (DatabaseOperationException ex)
            {
                Logger.Error(ex, $"Failed to log transaction: Type={type}, Account={account.AccNo}, Amount={amount}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"An unexpected error occurred while logging transaction: Type={type}, Account={account.AccNo}, Amount={amount}");
                throw new DatabaseOperationException("Failed to log transaction due to an unexpected error", ex);
            }
        }

        public IAccount GetAccount(string accNo)
        {
            if (string.IsNullOrEmpty(accNo))
            {
                throw new ArgumentNullException(nameof(accNo), "Account number cannot be null or empty");
            }

            var account = _accountDAO.GetAccount(accNo);
            if (account == null)
            {
                Logger.Warn($"Account not found: {accNo}");
                throw new AccountDoesNotExistException($"Account {accNo} does not exist");
            }

            if (account.Policy == null)
            {
                Logger.Warn($"Policy is null for account {accNo}. Attempting to create a new policy.");
                try
                {
                    account.Policy = _policyFactory.CreatePolicy(account.GetAccType().ToString(), account.PrivilegeType.ToString());
                    _accountDAO.UpdateAccount(account); // Save the updated account with the new policy
                    Logger.Info($"New policy created and assigned to account {accNo}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Failed to create policy for account {accNo}");
                    throw new InvalidOperationException($"Failed to create policy for account {accNo}", ex);
                }
            }

            return account;
        }
    }
}