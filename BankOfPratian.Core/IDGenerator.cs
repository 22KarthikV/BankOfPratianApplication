using System;
using System.Configuration;
using System.Data.SqlClient;

namespace BankOfPratian.Core
{
    public static class IDGenerator
    {
        public static string GenerateID(AccountType accType)
        {
            string prefix = accType == AccountType.SAVINGS ? "SAV" : "CUR";
            int nextID = GetNextID(accType);
            return $"{prefix}{nextID}";
        }

        public static int GenerateID()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BankOfPratianDB"].ConnectionString;
            string query = "SELECT ISNULL(MAX(TransID), 0) + 1 FROM TRANSACTION";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                int nextID = (int)command.ExecuteScalar();
                connection.Close();

                return nextID;
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