using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BankOfPratian.Core;
using BankOfPratian.Core.Exceptions;
using NLog;

namespace BankOfPratian.DataAccess
{
    public class ExternalTransferDAO : IExternalTransferDAO
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public ExternalTransferDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateExternalTransfer(ExternalTransfer transfer)
        {
            if (transfer == null)
            {
                throw new ArgumentNullException(nameof(transfer), "External transfer cannot be null");
            }

            if (transfer.FromAccount == null)
            {
                throw new ArgumentException("FromAccount cannot be null", nameof(transfer));
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    
                    var command = new SqlCommand(
                        "INSERT INTO ExternalTransfers (TransID, FromAccountNo, ToExternalAcc, Amount, TransactionDate, Status) " +
                        "VALUES (@TransID, @FromAccountNo, @ToExternalAcc, @Amount, @TransactionDate, @Status)", connection);

                    command.Parameters.AddWithValue("@TransID", transfer.TransID);
                    command.Parameters.AddWithValue("@FromAccountNo", transfer.FromAccount.AccNo);
                    command.Parameters.AddWithValue("@ToExternalAcc", transfer.ToExternalAcc ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Amount", transfer.Amount);
                    command.Parameters.AddWithValue("@TransactionDate", transfer.TranDate);
                    command.Parameters.AddWithValue("@Status", transfer.Status.ToString());

                    command.ExecuteNonQuery();
                    Logger.Info($"External transfer created: TransID={transfer.TransID}, FromAccountNo={transfer.FromAccount.AccNo}, Amount={transfer.Amount}");
                }
                catch (SqlException ex)
                {
                    Logger.Error(ex, $"SQL error occurred while creating external transfer: TransID={transfer.TransID}");
                    throw new DAOException("Error creating external transfer", ex);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Unexpected error occurred while creating external transfer: TransID={transfer.TransID}");
                    throw new DAOException("Unexpected error creating external transfer", ex);
                }
            }
        }


        public ExternalTransfer GetExternalTransfer(int transID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM ExternalTransfers WHERE TransID = @TransID", connection);
                command.Parameters.AddWithValue("@TransID", transID);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return CreateExternalTransferFromReader(reader);
                    }
                }
            }
            return null;
        }

        public List<ExternalTransfer> GetOpenExternalTransfers()
        {
            var transfers = new List<ExternalTransfer>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM ExternalTransfers WHERE Status = 'OPEN'", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transfers.Add(CreateExternalTransferFromReader(reader));
                    }
                }
            }
            return transfers;
        }

        public void UpdateExternalTransfer(ExternalTransfer transfer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "UPDATE ExternalTransfers SET Status = @Status WHERE TransID = @TransID",
                    connection);
                command.Parameters.AddWithValue("@Status", transfer.Status.ToString());
                command.Parameters.AddWithValue("@TransID", transfer.TransID);

                command.ExecuteNonQuery();
            }
        }

        private ExternalTransfer CreateExternalTransferFromReader(SqlDataReader reader)
        {
            return new ExternalTransfer
            {
                TransID = (int)reader["TransID"],
                FromAccount = null, // We'll set this later
                FromAccountNo = reader["FromAccountNo"].ToString(),
                ToExternalAcc = reader["ToExternalAcc"].ToString(),
                Amount = Convert.ToDouble(reader["Amount"]),
                TranDate = Convert.ToDateTime(reader["TransactionDate"]),
                Status = (TransactionStatus)Enum.Parse(typeof(TransactionStatus), reader["Status"].ToString())
            };
        }

        public List<ExternalTransfer> GetAllExternalTransfers()
        {
            var transfers = new List<ExternalTransfer>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM ExternalTransfers", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transfers.Add(CreateExternalTransferFromReader(reader));
                    }
                }
            }
            return transfers;
        }
    }


}