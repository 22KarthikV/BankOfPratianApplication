using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using NLog;
using System.Configuration;

namespace BankOfPratian.DataAccess
{
    public class AccountDAO : IAccountDAO
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;
        private readonly IPolicyFactory _policyFactory;

        public AccountDAO(string connectionString)
        {
            _connectionString = connectionString;
        }
        public AccountDAO(string connectionString, IPolicyFactory policyFactory)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString)); ;
            _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
        }

        public void CreateAccount(IAccount account)
        {
            const string sql = @"
        INSERT INTO ACCOUNT (accNo, name, pin, active, dtOfOpening, balance, privilegeType, accType)
        VALUES (@accNo, @name, @pin, @active, @dtOfOpening, @balance, @privilegeType, @accType)";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@accNo", SqlDbType.VarChar, 15).Value = account.AccNo;
                    command.Parameters.Add("@name", SqlDbType.VarChar, 30).Value = account.Name;
                    command.Parameters.Add("@pin", SqlDbType.VarChar, 4).Value = account.Pin;
                    command.Parameters.Add("@active", SqlDbType.Bit).Value = account.Active;
                    command.Parameters.Add("@dtOfOpening", SqlDbType.Date).Value = account.DateOfOpening;
                    command.Parameters.Add("@balance", SqlDbType.Float).Value = account.Balance;
                    command.Parameters.Add("@privilegeType", SqlDbType.VarChar, 15).Value = account.PrivilegeType.ToString();
                    command.Parameters.Add("@accType", SqlDbType.VarChar, 15).Value = account.GetAccType().ToString();

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                Logger.Info($"Account created in database: {account.AccNo}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error creating account in database: {account.AccNo}");
                throw new DatabaseOperationException("Error creating account", ex);
            }
        }

        public void Insert(IAccount account)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;
            string query = "INSERT INTO ACCOUNT (accNo, name, pin, active, dtOfOpening, balance, privilegeType, accType) " +
                           "VALUES (@accNo, @name, @pin, @active, @dtOfOpening, @balance, @privilegeType, @accType)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@accNo", account.AccNo);
                command.Parameters.AddWithValue("@name", account.Name);
                command.Parameters.AddWithValue("@pin", account.Pin);
                command.Parameters.AddWithValue("@active", account.Active);
                command.Parameters.AddWithValue("@dtOfOpening", account.DateOfOpening);
                command.Parameters.AddWithValue("@balance", account.Balance);
                command.Parameters.AddWithValue("@privilegeType", account.PrivilegeType.ToString());
                command.Parameters.AddWithValue("@accType", account.GetAccType());

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void UpdateAccount(IAccount account)
        {
            const string sql = @"
                UPDATE ACCOUNT 
                SET name = @name, pin = @pin, active = @active, balance = @balance, privilegeType = @privilegeType
                WHERE accNo = @accNo";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@accNo", SqlDbType.VarChar, 15).Value = account.AccNo;
                    command.Parameters.Add("@name", SqlDbType.VarChar, 30).Value = account.Name;
                    command.Parameters.Add("@pin", SqlDbType.VarChar, 4).Value = account.Pin;
                    command.Parameters.Add("@active", SqlDbType.Bit).Value = account.Active;
                    command.Parameters.Add("@balance", SqlDbType.Float).Value = account.Balance;
                    command.Parameters.Add("@privilegeType", SqlDbType.VarChar, 15).Value = account.PrivilegeType.ToString();

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                Logger.Info($"Account updated in database: {account.AccNo}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error updating account in database: {account.AccNo}");
                throw new DatabaseOperationException("Error updating account", ex);
            }
        }

        public IAccount GetAccount(string accNo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Account WHERE AccNo = @AccNo", connection);
                command.Parameters.AddWithValue("@AccNo", accNo);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return CreateAccountFromReader(reader);
                    }
                }
            }
            return null;
        }


        private IAccount CreateAccountFromReader(SqlDataReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            string accType = reader["AccType"].ToString();
            IAccount account;

            switch (accType.ToUpper())
            {
                case "SAVINGS":
                    account = new SavingsAccount(reader);
                    break;
                case "CURRENT":
                    account = new CurrentAccount(reader);
                    break;
                default:
                    throw new ArgumentException($"Unknown account type: {accType}");
            }

            // Create and assign the policy
            account.Policy = _policyFactory.CreatePolicy(accType, account.PrivilegeType.ToString());

            return account;
        }


        public int GetTotalAccountCount()
        {
            const string sql = "SELECT COUNT(*) FROM ACCOUNT";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting total account count");
                throw new DatabaseOperationException("Error getting total account count", ex);
            }
        }

        public Dictionary<AccountType, int> GetAccountTypeCount()
        {
            const string sql = "SELECT accType, COUNT(*) as Count FROM ACCOUNT GROUP BY accType";

            try
            {
                var result = new Dictionary<AccountType, int>();
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var accType = (AccountType)Enum.Parse(typeof(AccountType), reader["accType"].ToString());
                            var count = (int)reader["Count"];
                            result[accType] = count;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting account type count");
                throw new DatabaseOperationException("Error getting account type count", ex);
            }
        }

        public double GetTotalBankWorth()
        {
            const string sql = "SELECT SUM(balance) FROM ACCOUNT";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDouble(result);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting total bank worth");
                throw new DatabaseOperationException("Error getting total bank worth", ex);
            }
        }

        private IAccount CreateAccountFromReader(IDataReader reader, AccountType accType)
        {
            switch (accType)
            {
                case AccountType.SAVINGS:
                    return new SavingsAccount(reader);
                case AccountType.CURRENT:
                    return new CurrentAccount(reader);
                default:
                    throw new InvalidAccountTypeException($"Invalid account type: {accType}");
            }
        }
    }
}