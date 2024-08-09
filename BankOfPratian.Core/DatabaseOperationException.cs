using System;

namespace BankOfPratian.Core.Exceptions
{
    public class DatabaseOperationException : ApplicationException
    {
        public DatabaseOperationException(string message) : base(message) { }
        public DatabaseOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}