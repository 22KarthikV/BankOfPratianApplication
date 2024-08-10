using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using NLog;
using BankOfPratian.Core;

namespace BankOfPratian.DataAccess
{
    public class TransactionDAO : ITransactionDAO
    {


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public TransactionDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Transaction> GetAllTransactions()
        {
            const string sql = @"
        SELECT t.TransID, t.TransactionType, t.accNo, t.TransDate, t.amount, t.status,
               a.name, a.pin, a.active, a.dtOfOpening, a.balance, a.privilegeType, a.accType
        FROM [TRANSACTION] t
        JOIN ACCOUNT a ON t.accNo = a.accNo
        ORDER BY t.TransDate DESC";

            var transactions = new List<Transaction>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var accType = (AccountType)Enum.Parse(typeof(AccountType), reader["accType"].ToString());
                            IAccount account = CreateAccountFromReader(reader, accType);

                            //added the below variable
                            var transactionType = (TransactionType)Enum.Parse(typeof(TransactionType), reader["TransactionType"].ToString());
                            var transaction = new Transaction
                            {
                                TransID = (int)reader["TransID"],
                                TranDate = (DateTime)reader["TransDate"],
                                Amount = (double)reader["amount"],
                                Status = (TransactionStatus)Enum.Parse(typeof(TransactionStatus), reader["status"].ToString()),
                                FromAccount = account,
                                //added the below type variable
                                Type = transactionType

                            };
                            transactions.Add(transaction);
                        }
                    }
                }
                Logger.Info($"Retrieved {transactions.Count} transactions in total");
                return transactions;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error retrieving all transactions");
                throw new DatabaseOperationException("Error retrieving all transactions", ex);
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

        public double GetDailyTransferAmount(string accNo, DateTime date)
        {
            const string sql = @"
        SELECT SUM(amount) 
        FROM [TRANSACTION]
        WHERE accNo = @accNo 
        AND TransactionType = 'TRANSFER' 
        AND CAST(TransDate AS DATE) = @date";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@accNo", SqlDbType.VarChar, 15).Value = accNo;
                    command.Parameters.Add("@date", SqlDbType.Date).Value = date;

                    connection.Open();
                    var result = command.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDouble(result);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error getting daily transfer amount for account: {accNo}");
                throw new DatabaseOperationException("Error getting daily transfer amount", ex);
            }
        }

        /*public void LogTransaction(Transaction transaction)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    // Generate a new unique ID
                    transaction.TransID = IDGenerator.GenerateTransactionID();


                    using (var command = new SqlCommand("INSERT INTO [TRANSACTION] (TransID, TransactionType, accNo, TransDate, amount, status) VALUES (@TransID, @TransactionType, @accNo, @TransDate, @amount, @status)", connection))
                    {
                        command.Parameters.AddWithValue("@TransID", transaction.TransID);
                        command.Parameters.AddWithValue("@TransactionType", transaction.Type.ToString());
                        command.Parameters.AddWithValue("@accNo", transaction.FromAccount.AccNo);
                        command.Parameters.AddWithValue("@TransDate", transaction.TranDate);
                        command.Parameters.AddWithValue("@amount", transaction.Amount);
                        command.Parameters.AddWithValue("@status", transaction.Status.ToString());

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to log transaction: Type={transaction.Type}, Account={transaction.FromAccount.AccNo}, Amount={transaction.Amount}");
                throw new DatabaseOperationException("Failed to log transaction", ex);
            }
        }*/

        public void LogTransaction(Transaction transaction)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("INSERT INTO [TRANSACTION] (TransID, TransactionType, accNo, TransDate, amount, status) VALUES (@TransID, @TransactionType, @accNo, @TransDate, @amount, @status)", connection))
                    {
                        command.Parameters.AddWithValue("@TransID", transaction.TransID);
                        command.Parameters.AddWithValue("@TransactionType", transaction.Type.ToString());
                        command.Parameters.AddWithValue("@accNo", transaction.FromAccount.AccNo);
                        command.Parameters.AddWithValue("@TransDate", transaction.TranDate);
                        command.Parameters.AddWithValue("@amount", transaction.Amount);
                        command.Parameters.AddWithValue("@status", transaction.Status.ToString());

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to log transaction: Type={transaction.Type}, Account={transaction.FromAccount.AccNo}, Amount={transaction.Amount}");
                throw new DatabaseOperationException("Failed to log transaction", ex);
            }
        }

        public List<Transaction> GetTransactionsByAccount(string accNo)
        {
            const string sql = @"
                SELECT TransID, TransactionType, accNo, TransDate, amount, status
                FROM [TRANSACTION]
                WHERE accNo = @accNo
                ORDER BY TransDate DESC";

            var transactions = new List<Transaction>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@accNo", SqlDbType.VarChar, 15).Value = accNo;

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var transaction = new Transaction
                            {
                                TransID = (int)reader["TransID"],
                                TranDate = (DateTime)reader["TransDate"],
                                Amount = (double)reader["amount"],
                                Status = (TransactionStatus)Enum.Parse(typeof(TransactionStatus), reader["status"].ToString())
                            };
                            transactions.Add(transaction);
                        }
                    }
                }
                return transactions;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error retrieving transactions for account: {accNo}");
                throw new DatabaseOperationException("Error retrieving transactions", ex);
            }
        }
    }
}