using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagementSystem.Data.Repositories
{
    public class AdminRepository : AdoNetRepositoryBase
    {
        public AdminRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<AdminUser?> AuthenticateAsync(string username, string password)
        {
            const string query = @"
SELECT TOP 1 AdminUserId, Username, PasswordHash, IsActive, CreatedAt
FROM AdminUsers
WHERE Username = @Username AND IsActive = 1;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username.Trim());

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var stored = GetNullableString(reader, "PasswordHash") ?? string.Empty;
            var hashed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password.Trim()))).ToLowerInvariant();

            if (!string.Equals(stored, password.Trim(), StringComparison.Ordinal) &&
                !string.Equals(stored, hashed, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new AdminUser
            {
                AdminUserId = GetInt32(reader, "AdminUserId"),
                Username = GetNullableString(reader, "Username") ?? string.Empty,
                PasswordHash = stored,
                IsActive = GetBoolean(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt")
            };
        }
    }
}
