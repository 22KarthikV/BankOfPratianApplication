using BankOfPratian.Core;
using System.Collections.Generic;

namespace BankOfPratian.DataAccess
{
    public interface IAccountDAO
    {
        void CreateAccount(IAccount account);
        void UpdateAccount(IAccount account);
        IAccount GetAccount(string accNo);
        int GetTotalAccountCount();
        Dictionary<AccountType, int> GetAccountTypeCount();
        double GetTotalBankWorth();

    }
}