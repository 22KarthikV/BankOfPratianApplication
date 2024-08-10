using System;

namespace BankOfPratian.Core.Exceptions
{
    public class InvalidAccountTypeException : ApplicationException
    {
        public InvalidAccountTypeException(string message) : base(message) { }
    }

    public class InvalidPinException : ApplicationException
    {
        public InvalidPinException(string message) : base(message) { }
    }

    public class InsufficientBalanceException : ApplicationException
    {
        public InsufficientBalanceException(string message) : base(message) { }
    }

    public class InactiveAccountException : ApplicationException
    {
        public InactiveAccountException(string message) : base(message) { }
    }

    public class AccountDoesNotExistException : ApplicationException
    {
        public AccountDoesNotExistException(string message) : base(message) { }
    }

    public class InvalidPrivilegeTypeException : ApplicationException
    {
        public InvalidPrivilegeTypeException(string message) : base(message) { }
    }

    public class TransactionNotFoundException : ApplicationException
    {
        public TransactionNotFoundException(string message) : base(message) { }
    }

    public class InvalidTransactionTypeException : ApplicationException
    {
        public InvalidTransactionTypeException(string message) : base(message) { }
    }

    public class DailyLimitExceededException : ApplicationException
    {
        public DailyLimitExceededException(string message) : base(message) { }
    }

    public class MinBalanceNeedsToBeMaintainedException : ApplicationException
    {
        public MinBalanceNeedsToBeMaintainedException(string message) : base(message) { }
    }

    public class UnableToOpenAccountException : ApplicationException
    {
        public UnableToOpenAccountException(string message) : base(message) { }
    }

    public class InvalidPolicyTypeException : ApplicationException
    {
        public InvalidPolicyTypeException(string message) : base(message) { }
    }

    public class InvalidPolicyException : ApplicationException
    {
        public InvalidPolicyException(string message) : base(message) { }
    }


    [Serializable]
    public class DAOException : ApplicationException
    {
        public DAOException()
        {
        }

        public DAOException(string? message) : base(message)
        {
        }

        public DAOException(string? message, Exception? innerException) : base(message, innerException)
        {
        }


    }

    [Serializable]
    public class ExternalTransferException : ApplicationException
    {
        public ExternalTransferException()
        {
        }

        public ExternalTransferException(string? message) : base(message)
        {
        }

        public ExternalTransferException(string? message, Exception? innerException) : base(message, innerException)
        {
        }


    }

    public class DatabaseOperationException : ApplicationException
    {
        public DatabaseOperationException(string message) : base(message) { }
        public DatabaseOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}