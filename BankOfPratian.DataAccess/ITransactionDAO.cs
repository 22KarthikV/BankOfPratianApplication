using System;
using System.Collections.Generic;
using BankOfPratian.Core;

namespace BankOfPratian.DataAccess
{
    public interface ITransactionDAO
    {
        List<Transaction> GetAllTransactions();
        double GetDailyTransferAmount(string accNo, DateTime date);
        void LogTransaction(Transaction transaction);
        List<Transaction> GetTransactionsByAccount(string accNo);
    }
}