using System.Security.Cryptography;
using System.Text;
using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class StaffRepository : AdoNetRepositoryBase
    {
        public StaffRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<StaffUser?> AuthenticateAsync(string username, string password)
        {
            const string query = @"
SELECT TOP 1 StaffUserId, FullName, Username, PasswordHash, Role, IsActive, CreatedAt
FROM StaffUsers
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
                    string.Equals(storedHash, submittedHash, StringComparison.Ordinal))
                {
                    return new StaffUser
                    {
                        StaffUserId = GetInt32(reader, "StaffUserId"),
                        FullName = GetNullableString(reader, "FullName") ?? string.Empty,
                        Username = GetNullableString(reader, "Username") ?? string.Empty,
                        PasswordHash = storedDirect,
                        Role = GetNullableString(reader, "Role") ?? "Staff",
                        IsActive = GetBoolean(reader, "IsActive"),
                        CreatedAt = GetDateTime(reader, "CreatedAt")
                    };
                }
            }

            return null;
        }

        public async Task<int> AddAsync(StaffUser staffUser)
        {
            const string query = @"
INSERT INTO StaffUsers (FullName, Username, PasswordHash, Role, IsActive)
OUTPUT INSERTED.StaffUserId
VALUES (@FullName, @Username, @PasswordHash, @Role, @IsActive);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FullName", staffUser.FullName.Trim());
            command.Parameters.AddWithValue("@Username", staffUser.Username.Trim());
            command.Parameters.AddWithValue("@PasswordHash", ComputeHash(staffUser.PasswordHash));
            command.Parameters.AddWithValue("@Role", string.IsNullOrWhiteSpace(staffUser.Role) ? "Staff" : staffUser.Role.Trim());
            command.Parameters.AddWithValue("@IsActive", staffUser.IsActive);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
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