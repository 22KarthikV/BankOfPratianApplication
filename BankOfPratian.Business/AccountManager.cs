using System;
using System.Collections.Generic;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using BankOfPratian.DataAccess;
using NLog;

namespace BankOfPratian.Business
{
    public class AccountManager
    {
        /*private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AccountPrivilegeManager _privilegeManager;
        private readonly IPolicyFactory _policyFactory;
        private readonly IAccountDAO _accountDAO; // changed here 
        private readonly ITransactionDAO _transactionDAO;*/

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AccountPrivilegeManager _privilegeManager;
        private readonly IAccountDAO _accountDAO;
        private readonly ITransactionDAO _transactionDAO;
        private readonly IPolicyFactory _policyFactory;

        public AccountManager(
            AccountPrivilegeManager privilegeManager,
            IAccountDAO accountDAO,
            ITransactionDAO transactionDAO,
            IPolicyFactory policyFactory)
        {
            _privilegeManager = privilegeManager;
            _accountDAO = accountDAO;
            _transactionDAO = transactionDAO;
            _policyFactory = policyFactory;
        }

        /*public AccountManager(AccountPrivilegeManager privilegeManager, IAccountDAO accountDAO, ITransactionDAO transactionDAO, IPolicyFactory policyFactory = null)
        {
            _privilegeManager = privilegeManager;
            _accountDAO = accountDAO;
            _transactionDAO = transactionDAO;
            _policyFactory = policyFactory ?? PolicyFactory.Instance; // Use instance if null
        }*/

        /*public AccountManager(AccountPrivilegeManager privilegeManager, IAccountDAO accountDAO, ITransactionDAO transactionDAO, PolicyFactory policyFactory = null)
        {
            _privilegeManager = privilegeManager;
            _accountDAO = accountDAO;
            _transactionDAO = transactionDAO;
            _policyFactory = policyFactory ?? PolicyFactory.Instance;
        }

        public AccountManager(IPolicyFactory policyFactory, IAccountDAO accountDAO, ITransactionDAO transactionDAO)
        {
            _policyFactory = policyFactory;
            _accountDAO = accountDAO;
            _transactionDAO = transactionDAO;
        }*/

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

                var policy = _policyFactory.CreatePolicy(accType.ToString(), privilegeType.ToString());
                Logger.Debug($"Policy created: MinBalance={policy.GetMinBalance()}, RateOfInterest={policy.GetRateOfInterest()}");
                account.Policy = policy;

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


        /*public bool Deposit(IAccount toAccount, double amount)
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
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error depositing to account");
                throw;
            }
        }

        public bool Withdraw(IAccount fromAccount, double amount, string pin)
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

                if (fromAccount.Balance - amount < fromAccount.Policy.GetMinBalance())
                {
                    throw new InsufficientBalanceException("Insufficient balance");
                }

                fromAccount.Balance -= amount;
                _accountDAO.UpdateAccount(fromAccount);
                Logger.Info($"Withdrawal of {amount} from account {fromAccount.AccNo}");
                LogTransaction(fromAccount, TransactionType.WITHDRAW, amount);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error withdrawing from account");
                throw;
            }
        }

        public bool TransferFunds(Transfer transfer)
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
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error transferring funds");
                throw;
            }
        }*/

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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error depositing to account");
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

        private double GetDailyTransferAmount(string accNo)
        {
            // Implement this method to retrieve the total transfer amount for the current day
            // You can use _transactionDAO to query the database for this information
            return _transactionDAO.GetDailyTransferAmount(accNo, DateTime.Today);
        }

        private void LogTransaction(IAccount account, TransactionType type, double amount)
        {
            var transaction = new Transaction
            {
                TransID = IDGenerator.GenerateID(),
                FromAccount = account,
                TranDate = DateTime.Now,
                Amount = amount,
                Status = TransactionStatus.CLOSED,
                Type = type  // Use the new Type property
            };

            try
            {
                _transactionDAO.LogTransaction(transaction);
                Logger.Info($"Transaction logged: Type={type}, Account={account.AccNo}, Amount={amount}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to log transaction: Type={type}, Account={account.AccNo}, Amount={amount}");
                throw new DatabaseOperationException("Failed to log transaction", ex);
            }
        }

        public IAccount GetAccount(string accNo)
        {
            try
            {
                var account = _accountDAO.GetAccount(accNo);
                if (account == null)
                {
                    throw new AccountDoesNotExistException($"Account {accNo} does not exist");
                }
                return account;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error retrieving account: {accNo}");
                throw;
            }
        }
    }
}