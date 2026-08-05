using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data
{
    /// <summary>
    /// This class helps in establishing a connection to the SQL Server database.
    /// We use ADO.NET (SqlConnection) throughout the project.
    /// </summary>
    public class DbConnectionHelper
    {
        private readonly IConfiguration _configuration;

        // IConfiguration is injected via Dependency Injection in Program.cs
        public DbConnectionHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Reads the connection string from appsettings.json
        /// </summary>
        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        /// <summary>
        /// Creates and returns a new SqlConnection object
        /// Make sure to wrap usages of this method in a 'using' statement.
        /// </summary>
        public SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }
    }
}
