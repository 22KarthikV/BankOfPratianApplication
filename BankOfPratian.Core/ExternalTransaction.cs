using System;

namespace BankOfPratian.Core
{
    public class ExternalTransaction
    {
        public string TransID { get; set; }
        public string TransactionType { get; set; }
        public IAccount FromAccount { get; set; }
        public string ToExternalAcc { get; set; }
        public DateTime TranDate { get; set; }
        public double Amount { get; set; }
        public TransactionStatus Status { get; set; }
    }
}