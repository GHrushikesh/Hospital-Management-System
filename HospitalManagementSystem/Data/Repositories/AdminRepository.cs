using System.Security.Cryptography;
using System.Text;
using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

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

            var submittedHash = ComputeHash(password);

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username.Trim());

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var storedPassword = GetNullableString(reader, "PasswordHash") ?? string.Empty;
                var storedDirect = storedPassword.Trim();
                var storedHash = ComputeHash(storedDirect);

                if (string.Equals(storedDirect, password.Trim(), StringComparison.Ordinal) ||
                    string.Equals(storedHash, submittedHash, StringComparison.Ordinal) ||
                    string.Equals(storedHash, ComputeHash(password.Trim()), StringComparison.Ordinal))
                {
                    return new AdminUser
                    {
                        AdminUserId = GetInt32(reader, "AdminUserId"),
                        Username = GetNullableString(reader, "Username") ?? string.Empty,
                        PasswordHash = storedDirect,
                        IsActive = GetBoolean(reader, "IsActive"),
                        CreatedAt = GetDateTime(reader, "CreatedAt")
                    };
                }
            }

            return null;
        }

        public static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input.Trim());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}