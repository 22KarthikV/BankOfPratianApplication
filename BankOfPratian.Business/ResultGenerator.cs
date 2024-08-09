using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using BankOfPratian.Core;
using BankOfPratian.DataAccess;
using NLog;
using System.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace BankOfPratian.Business
{
    public static class ResultGenerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly AccountDAO _accountDAO;
        private static readonly TransactionDAO _transactionDAO;

        static ResultGenerator()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;
            _accountDAO = new AccountDAO(connectionString);
            _transactionDAO = new TransactionDAO(connectionString);
        }

        public static void PrintAllLogTransactions()
        {
            try
            {
                var allTransactions = _transactionDAO.GetAllTransactions();
                foreach (var transaction in allTransactions)
                {
                    Console.WriteLine($"Account: {transaction.FromAccount.AccNo}, Type: {transaction.GetType().Name}, Date: {transaction.TranDate}, Amount: {transaction.Amount}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error printing all log transactions");
            }
        }

        public static void PrintAllLogTransactions(string accountId)
        {
            try
            {
                var transactions = _transactionDAO.GetTransactionsByAccount(accountId);
                Console.WriteLine("Recent Transactions:");
                Console.WriteLine("Date                 | Type     | Amount");
                Console.WriteLine("---------------------|----------|-------");
                foreach (var transaction in transactions.OrderByDescending(t => t.TranDate).Take(10))
                {
                    Console.WriteLine($"{transaction.TranDate,-20:g}| {transaction.GetType().Name,-9}| {transaction.Amount,7:C2}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error printing transactions for account: {accountId}");
                Console.WriteLine("Error retrieving transactions.");
            }
        }

        public static void PrintAllLogTransactions(TransactionType transactionType)
        {
            try
            {
                var allTransactions = _transactionDAO.GetAllTransactions();
                var filteredTransactions = allTransactions.Where(t => t.GetType().Name == transactionType.ToString());
                foreach (var transaction in filteredTransactions)
                {
                    Console.WriteLine($"Account: {transaction.FromAccount.AccNo}, Date: {transaction.TranDate}, Amount: {transaction.Amount}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error printing log transactions for transaction type {transactionType}");
            }
        }

        public static int GetTotalNoOfAccounts()
        {
            try
            {
                int totalAccounts = _accountDAO.GetTotalAccountCount();
                Console.WriteLine($"Total number of accounts: {totalAccounts}");
                return totalAccounts;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting total number of accounts");
                Console.WriteLine("Error retrieving total number of accounts.");
                return 0;
            }
        }

        public static void DisplayNoOfAccTypeWise()
        {
            try
            {
                var accountTypeCounts = _accountDAO.GetAccountTypeCount();
                Console.WriteLine("Account Type | No Of Accounts");
                Console.WriteLine("-------------|----------------");
                foreach (var (accountType, count) in accountTypeCounts)
                {
                    Console.WriteLine($"{accountType,-13}| {count,15}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying number of accounts by type");
                Console.WriteLine("Error retrieving account type counts.");
            }
        }

        public static void DispTotalWorthOfBank()
        {
            try
            {
                double totalWorth = _accountDAO.GetTotalBankWorth();
                Console.WriteLine($"Total balance available: {totalWorth:C2}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying total worth of bank");
                Console.WriteLine("Error retrieving total bank worth.");
            }
        }

        public static void DispPolicyInfo()
        {
            try
            {
                var policyInfo = PolicyFactory.Instance.GetAllPolicies();
                Console.WriteLine("Policy Type      | Minimum Balance | Rate Of Interest");
                Console.WriteLine("-----------------|-----------------|------------------");
                foreach (var (policyType, policy) in policyInfo)
                {
                    Console.WriteLine($"{policyType,-16}| {policy.GetMinBalance(),15:C2} | {policy.GetRateOfInterest(),17:P2}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying policy information");
                Console.WriteLine("Error retrieving policy information.");
            }
        }

        public static void DisplayAllTransfers()
        {
            try
            {
                var allTransactions = _transactionDAO.GetAllTransactions();
                var transfers = allTransactions.Where(t => t.GetType().Name == "Transfer");
                Console.WriteLine("From       | To         | Date                 | Amount");
                Console.WriteLine("-----------|------------|----------------------|-------");
                foreach (var transfer in transfers)
                {
                    Console.WriteLine($"{transfer.FromAccount.AccNo,-10}| {"To Account",-10}| {transfer.TranDate,-20:g}| {transfer.Amount,7:C2}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying all transfers");
            }
        }

        public static void DisplayAllWithdrawals()
        {
            try
            {
                var allTransactions = _transactionDAO.GetAllTransactions();
                var withdrawals = allTransactions.Where(t => t.GetType().Name == "Withdraw");
                Console.WriteLine("From       | Date                 | Amount");
                Console.WriteLine("-----------|----------------------|-------");
                foreach (var withdrawal in withdrawals)
                {
                    Console.WriteLine($"{withdrawal.FromAccount.AccNo,-10}| {withdrawal.TranDate,-20:g}| {withdrawal.Amount,7:C2}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying all withdrawals");
            }
        }

        public static void DisplayAllDeposits()
        {
            try
            {
                var allTransactions = _transactionDAO.GetAllTransactions();
                var deposits = allTransactions.Where(t => t.GetType().Name == "Deposit");
                Console.WriteLine("To         | Date                 | Amount");
                Console.WriteLine("-----------|----------------------|-------");
                foreach (var deposit in deposits)
                {
                    Console.WriteLine($"{deposit.FromAccount.AccNo,-10}| {deposit.TranDate,-20:g}| {deposit.Amount,7:C2}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying all deposits");
            }
        }

        public static void DisplayAllTransactionsForToday()
        {
            try
            {
                var allTransactions = _transactionDAO.GetAllTransactions();
                var today = DateTime.Today;
                var todayTransactions = allTransactions.Where(t => t.TranDate.Date == today);
                Console.WriteLine("From       | To         | Date                 | Amount | Type");
                Console.WriteLine("-----------|------------|----------------------|--------|------");
                foreach (var transaction in todayTransactions)
                {
                    Console.WriteLine($"{transaction.FromAccount.AccNo,-10}| {"To Account",-10}| {transaction.TranDate,-20:g}| {transaction.Amount,7:C2}| {transaction.GetType().Name,-5}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying all transactions for today");
            }
        }

        public static void DisplayAllTransactions()
        {
            try
            {
                var allTransactions = _transactionDAO.GetAllTransactions();
                Console.WriteLine("All Transactions");
                Console.WriteLine("Account   | Type     | Date                 | Amount");
                Console.WriteLine("----------|----------|----------------------|--------");
                foreach (var transaction in allTransactions.OrderByDescending(t => t.TranDate))
                {
                    Console.WriteLine($"{transaction.FromAccount.AccNo,-10}| {transaction.GetType().Name,-9}| {transaction.TranDate,-20:g}| {transaction.Amount,7:C2}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error displaying all transactions");
            }
        }
    }
}