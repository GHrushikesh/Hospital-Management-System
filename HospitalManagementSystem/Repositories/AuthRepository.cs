using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.Repositories
{
    public class AuthRepository
    {
        private readonly DbConnectionHelper _dbHelper;

        public AuthRepository(DbConnectionHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Method to verify if the username and password match a record in the database
        public bool ValidateAdminUser(string username, string password)
        {
            bool isValid = false;

            // 'using' ensures the database connection is closed automatically after executing
            using (SqlConnection conn = _dbHelper.GetConnection())
            {
                // Parameterized query to prevent SQL Injection attacks!
                string query = "SELECT COUNT(1) FROM Users WHERE Username = @username AND Password = @password AND Role = 'Admin'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Binding parameters securely
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    // Open connection, execute scalar (returns the first column of the first row), then close
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    // If count is 1, a matching user was found
                    if (count == 1)
                    {
                        isValid = true;
                    }
                }
            }

            return isValid;
        }
    }
}
