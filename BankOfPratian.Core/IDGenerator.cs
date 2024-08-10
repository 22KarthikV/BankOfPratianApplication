using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;

namespace BankOfPratian.Core
{
    public static class IDGenerator
    {
        private static int _lastTransactionId = 0;
        private static readonly object _lock = new object();

        public static string GenerateID(AccountType accType)
        {
            string prefix = accType == AccountType.SAVINGS ? "SAV" : "CUR";
            int nextID = GetNextID(accType);
            return $"{prefix}{nextID}";
        }

        public static int GenerateTransactionID(bool isExternalTransfer = false)
        {
            lock (_lock)
            {
                if (_lastTransactionId == 0)
                {
                    InitializeLastTransactionId();
                }
                int newId = ++_lastTransactionId;

                // Log the ID generation for debugging
                Console.WriteLine($"Generated TransID: {newId}, IsExternal: {isExternalTransfer}");

                return newId;
            }
        }

        private static void InitializeLastTransactionId()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;
            string query = @"
                SELECT ISNULL(MAX(TransID), 0) 
                FROM (
                    SELECT TransID FROM [TRANSACTION]
                    UNION ALL
                    SELECT TransID FROM ExternalTransfers
                ) AS AllTransactions";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                _lastTransactionId = (int)command.ExecuteScalar();
                connection.Close();
            }
        }

        private static int GetNextID(AccountType accType)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;
            string query = "SELECT ISNULL(MAX(CAST(SUBSTRING(accNo, 4, LEN(accNo) - 3) AS INT)), 0) + 1 FROM ACCOUNT WHERE accType = @accType";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@accType", accType.ToString());
                connection.Open();
                int nextID = (int)command.ExecuteScalar();
                connection.Close();
                return nextID;
            }
        }
    }
}

/*using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;

namespace BankOfPratian.Core
{
    public static class IDGenerator
    {
        private static int _lastTransactionId = 0;
        private static readonly object _lock = new object();

        public static string GenerateID(AccountType accType)
        {
            string prefix = accType == AccountType.SAVINGS ? "SAV" : "CUR";
            int nextID = GetNextID(accType);
            return $"{prefix}{nextID}";
        }

        *//*public static int GenerateTransactionID()
        {
            if (_lastTransactionId == 0)
            {
                InitializeLastTransactionId();
            }
            
            return Interlocked.Increment(ref _lastTransactionId);
        }*//*

        public static int GenerateTransactionID()
        {
            lock (_lock)
            {
                if (_lastTransactionId == 0)
                {
                    InitializeLastTransactionId();
                }
                return ++_lastTransactionId;
            }
        }

        private static void InitializeLastTransactionId()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;
            string query = @"
                SELECT ISNULL(MAX(TransID), 0) 
                FROM (
                    SELECT TransID FROM [TRANSACTION]
                    UNION ALL
                    SELECT TransID FROM ExternalTransfers
                ) AS AllTransactions";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                _lastTransactionId = (int)command.ExecuteScalar();
                connection.Close();
            }
        }

        private static int GetNextID(AccountType accType)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;
            string query = "SELECT ISNULL(MAX(CAST(SUBSTRING(accNo, 4, LEN(accNo) - 3) AS INT)), 0) + 1 FROM ACCOUNT WHERE accType = @accType";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@accType", accType.ToString());
                connection.Open();
                int nextID = (int)command.ExecuteScalar();
                connection.Close();
                return nextID;
            }
        }
    }
}*/