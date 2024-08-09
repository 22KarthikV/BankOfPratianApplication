using System;

namespace BankOfPratian.Core
{
    public enum PrivilegeType
    {
        REGULAR,
        GOLD,
        PREMIUM
    }

    public enum AccountType
    {
        SAVINGS,
        CURRENT
    }

    public enum TransactionType
    {
        DEPOSIT,
        WITHDRAW,
        TRANSFER,
        EXTERNALTRANSFER
    }

    public enum TransactionStatus
    {
        OPEN,
        CLOSED
    }
}