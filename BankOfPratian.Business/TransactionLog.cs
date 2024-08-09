using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using NLog;

namespace BankOfPratian.Business
{
    public static class TransactionLog
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<TransactionType, List<Transaction>>> Log =
            new ConcurrentDictionary<string, ConcurrentDictionary<TransactionType, List<Transaction>>>();

        public static IDictionary<string, IDictionary<TransactionType, IList<Transaction>>> GetTransactions()
        {
            if (Log.IsEmpty)
            {
                throw new TransactionNotFoundException("No transactions found");
            }
            return Log.ToDictionary(
                kvp => kvp.Key,
                kvp => (IDictionary<TransactionType, IList<Transaction>>)kvp.Value.ToDictionary(
                    innerKvp => innerKvp.Key,
                    innerKvp => (IList<Transaction>)innerKvp.Value
                )
            );
        }

        public static IDictionary<TransactionType, IList<Transaction>> GetTransactions(string accNo)
        {
            if (!Log.TryGetValue(accNo, out var accountTransactions))
            {
                throw new TransactionNotFoundException($"No transactions found for account {accNo}");
            }
            return accountTransactions.ToDictionary(kvp => kvp.Key, kvp => (IList<Transaction>)kvp.Value);
        }

        public static IList<Transaction> GetTransactions(string accNo, TransactionType type)
        {
            if (!Log.TryGetValue(accNo, out var accountTransactions))
            {
                throw new TransactionNotFoundException($"No transactions found for account {accNo}");
            }
            if (!accountTransactions.TryGetValue(type, out var transactions))
            {
                throw new InvalidTransactionTypeException($"No transactions of type {type} found for account {accNo}");
            }
            return transactions;
        }

        public static void LogTransaction(string accNo, TransactionType type, Transaction transaction)
        {
            try
            {
                var accountTransactions = Log.GetOrAdd(accNo, _ => new ConcurrentDictionary<TransactionType, List<Transaction>>());
                var typeTransactions = accountTransactions.GetOrAdd(type, _ => new List<Transaction>());
                typeTransactions.Add(transaction);
                Logger.Info($"Transaction logged: Account {accNo}, Type {type}, Amount {transaction.Amount}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error logging transaction for account {accNo}");
                throw;
            }
        }
    }
}