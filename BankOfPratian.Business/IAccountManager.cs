using BankOfPratian.Core;

namespace BankOfPratian.Business
{
    public interface IAccountManager
    {
        IAccount CreateAccount(string name, string pin, double balance, PrivilegeType privilegeType, AccountType accType);
        void Deposit(IAccount toAccount, double amount);
        void Withdraw(IAccount fromAccount, double amount, string pin);
        void TransferFunds(Transfer transfer);
        IAccount GetAccount(string accNo);
    }
}