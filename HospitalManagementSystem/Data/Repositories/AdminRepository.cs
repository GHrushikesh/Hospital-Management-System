using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class AdminRepository : AdoNetRepositoryBase
    {
        public AdminRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<AdminUser?> AuthenticateAsync(string username, string passwordHash)
        {
            const string query = @"
SELECT TOP 1 AdminUserId, Username, PasswordHash, IsActive, CreatedAt
FROM AdminUsers
WHERE Username = @Username AND PasswordHash = @PasswordHash AND IsActive = 1;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username.Trim());
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new AdminUser
            {
                AdminUserId = GetInt32(reader, "AdminUserId"),
                Username = GetNullableString(reader, "Username") ?? string.Empty,
                PasswordHash = GetNullableString(reader, "PasswordHash") ?? string.Empty,
                IsActive = GetBoolean(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt")
            };
        }
    }
}