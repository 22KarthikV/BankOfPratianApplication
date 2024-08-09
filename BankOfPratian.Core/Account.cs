using System;
using System.Data;

namespace BankOfPratian.Core
{
    public abstract class Account : IAccount
    {
        public string AccNo { get; protected set; }
        public string Name { get; set; }
        public string Pin { get; set; }
        public bool Active { get; set; }
        public DateTime DateOfOpening { get; set; }
        public double Balance { get; set; }
        public PrivilegeType PrivilegeType { get; set; }
        public IPolicy Policy { get; set; }

        public abstract AccountType GetAccType();
        public abstract bool Open();
        public abstract bool Close();

        public virtual void InitializeFromReader(IDataReader reader)
        {
            AccNo = reader["accNo"].ToString();
            Name = reader["name"].ToString();
            Pin = reader["pin"].ToString();
            Active = (bool)reader["active"];
            DateOfOpening = (DateTime)reader["dtOfOpening"];
            Balance = (double)reader["balance"];
            PrivilegeType = (PrivilegeType)Enum.Parse(typeof(PrivilegeType), reader["privilegeType"].ToString());
        }
    }

    public class SavingsAccount : Account
    {
        public SavingsAccount()
        {
            AccNo = IDGenerator.GenerateID(AccountType.SAVINGS);
        }

        public SavingsAccount(IDataReader reader) : this()
        {
            InitializeFromReader(reader);
        }

        public override AccountType GetAccType() => AccountType.SAVINGS;

        public override bool Open()
        {
            Active = true;
            DateOfOpening = DateTime.Now;
            return true;
        }

        public override bool Close()
        {
            Active = false;
            Balance = 0;
            return true;
        }
    }

    public class CurrentAccount : Account
    {
        public CurrentAccount()
        {
            AccNo = IDGenerator.GenerateID(AccountType.CURRENT);
        }

        public CurrentAccount(IDataReader reader) : this()
        {
            InitializeFromReader(reader);
        }

        public override AccountType GetAccType() => AccountType.CURRENT;

        public override bool Open()
        {
            Active = true;
            DateOfOpening = DateTime.Now;
            return true;
        }

        public override bool Close()
        {
            Active = false;
            Balance = 0;
            return true;
        }
    }
}