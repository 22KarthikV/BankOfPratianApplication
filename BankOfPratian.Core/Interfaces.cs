using System;

namespace BankOfPratian.Core
{
    public interface IAccount
    {
        string AccNo { get; }
        string Name { get; set; }
        string Pin { get; set; }
        bool Active { get; set; }
        DateTime DateOfOpening { get; set; }
        double Balance { get; set; }
        PrivilegeType PrivilegeType { get; set; }
        IPolicy Policy { get; set; }
        AccountType GetAccType();
        bool Open();
        bool Close();

    }

    public interface IPolicy
    {
        double GetMinBalance();
        double GetRateOfInterest();
    }

    public interface IExternalBankService
    {
        bool Deposit(string accId, double amt);
    }
}