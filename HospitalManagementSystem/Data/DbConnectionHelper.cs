using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data
{
    public class DbConnectionHelper
    {
        private readonly IConfiguration _configuration;

        public DbConnectionHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }
    }
}
