﻿using System;



namespace BankOfPratian.Core
{
    public class Transaction
    {
        public int TransID { get; set; }
        public IAccount FromAccount { get; set; }
        public DateTime TranDate { get; set; }
        public double Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public TransactionType Type { get; set; }

        public Transaction()
        {
            Status = TransactionStatus.CLOSED;
        }
    }

    public class ExternalTransfer : Transaction
    {
        public string ToExternalAcc { get; set; }
        public string FromAccPin { get; set; }

        public string FromAccountNo { get; set; }
        public ExternalTransfer()
        {
            Type = TransactionType.EXTERNALTRANSFER;
            Status = TransactionStatus.OPEN;
        }
    }

    public class Transfer : Transaction
    {
        public IAccount FromAcc { get; set; }
        public IAccount ToAcc { get; set; }
        public double Amount { get; set; }
        public string Pin { get; set; }

        public Transfer()
        {
            Type = TransactionType.TRANSFER;
            Status = TransactionStatus.CLOSED;
        }
    }

    public class ExternalAccount
    {
        public string AccNo { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
    }
}