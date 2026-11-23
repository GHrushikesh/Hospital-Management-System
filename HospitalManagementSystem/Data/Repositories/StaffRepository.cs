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
SELECT TOP 1 StaffUserId, FullName, Username, PasswordHash, Role, PhoneNumber, Email, Department, JobTitle, EmployeeId, Qualification, AssignedRoomOrWard, ShiftTiming, HiredByDoctorId, HiredByDoctorName, IsActive, CreatedAt
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
                    string.Equals(storedHash, submittedHash, StringComparison.Ordinal) ||
                    string.Equals(storedDirect, submittedHash, StringComparison.OrdinalIgnoreCase))
                {
                    return MapStaffUser(reader);
                }
            }

            return null;
        }

        public async Task<int> AddAsync(StaffUser staffUser)
        {
            const string query = @"
INSERT INTO StaffUsers (FullName, Username, PasswordHash, Role, PhoneNumber, Email, Department, JobTitle, EmployeeId, Qualification, AssignedRoomOrWard, ShiftTiming, HiredByDoctorId, HiredByDoctorName, IsActive)
OUTPUT INSERTED.StaffUserId
VALUES (@FullName, @Username, @PasswordHash, @Role, @PhoneNumber, @Email, @Department, @JobTitle, @EmployeeId, @Qualification, @AssignedRoomOrWard, @ShiftTiming, @HiredByDoctorId, @HiredByDoctorName, @IsActive);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FullName", staffUser.FullName.Trim());
            command.Parameters.AddWithValue("@Username", staffUser.Username.Trim());
            command.Parameters.AddWithValue("@PasswordHash", ComputeHash(staffUser.PasswordHash));
            command.Parameters.AddWithValue("@Role", string.IsNullOrWhiteSpace(staffUser.Role) ? "Staff" : staffUser.Role.Trim());
            command.Parameters.AddWithValue("@PhoneNumber", staffUser.PhoneNumber.Trim());
            command.Parameters.AddWithValue("@Email", staffUser.Email.Trim());
            command.Parameters.AddWithValue("@Department", staffUser.Department.Trim());
            command.Parameters.AddWithValue("@JobTitle", staffUser.JobTitle.Trim());
            command.Parameters.AddWithValue("@EmployeeId", DbValue(staffUser.EmployeeId));
            command.Parameters.AddWithValue("@Qualification", string.IsNullOrWhiteSpace(staffUser.Qualification) ? "" : staffUser.Qualification.Trim());
            command.Parameters.AddWithValue("@AssignedRoomOrWard", string.IsNullOrWhiteSpace(staffUser.AssignedRoomOrWard) ? "" : staffUser.AssignedRoomOrWard.Trim());
            command.Parameters.AddWithValue("@ShiftTiming", string.IsNullOrWhiteSpace(staffUser.ShiftTiming) ? "" : staffUser.ShiftTiming.Trim());
            command.Parameters.AddWithValue("@HiredByDoctorId", (object?)staffUser.HiredByDoctorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@HiredByDoctorName", DbValue(staffUser.HiredByDoctorName));
            command.Parameters.AddWithValue("@IsActive", staffUser.IsActive);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IReadOnlyList<StaffUser>> GetAllAsync()
        {
            const string query = @"
SELECT StaffUserId, FullName, Username, PasswordHash, Role, PhoneNumber, Email, Department, JobTitle, EmployeeId, Qualification, AssignedRoomOrWard, ShiftTiming, HiredByDoctorId, HiredByDoctorName, IsActive, CreatedAt
FROM StaffUsers
ORDER BY FullName;";

            List<StaffUser> staffList = new List<StaffUser>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                staffList.Add(MapStaffUser(reader));
            }

            return staffList;
        }

        public async Task<IReadOnlyList<StaffUser>> GetStaffByDoctorIdAsync(int doctorId)
        {
            const string query = @"
SELECT StaffUserId, FullName, Username, PasswordHash, Role, PhoneNumber, Email, Department, JobTitle, EmployeeId, Qualification, AssignedRoomOrWard, ShiftTiming, HiredByDoctorId, HiredByDoctorName, IsActive, CreatedAt
FROM StaffUsers
WHERE HiredByDoctorId = @DoctorId
ORDER BY CreatedAt DESC;";

            List<StaffUser> staffList = new List<StaffUser>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                staffList.Add(MapStaffUser(reader));
            }

            return staffList;
        }

        public async Task<StaffUser?> GetByIdAsync(int staffUserId)
        {
            const string query = @"
SELECT TOP 1 StaffUserId, FullName, Username, PasswordHash, Role, PhoneNumber, Email, Department, JobTitle, EmployeeId, Qualification, AssignedRoomOrWard, ShiftTiming, HiredByDoctorId, HiredByDoctorName, IsActive, CreatedAt
FROM StaffUsers
WHERE StaffUserId = @StaffUserId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@StaffUserId", staffUserId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapStaffUser(reader);
        }

        public async Task<bool> DeleteAsync(int staffUserId)
        {
            const string query = @"DELETE FROM StaffUsers WHERE StaffUserId = @StaffUserId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@StaffUserId", staffUserId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static StaffUser MapStaffUser(SqlDataReader reader)
        {
            return new StaffUser
            {
                StaffUserId = GetInt32(reader, "StaffUserId"),
                FullName = GetNullableString(reader, "FullName") ?? string.Empty,
                Username = GetNullableString(reader, "Username") ?? string.Empty,
                PasswordHash = GetNullableString(reader, "PasswordHash") ?? string.Empty,
                Role = GetNullableString(reader, "Role") ?? "Staff",
                PhoneNumber = GetNullableString(reader, "PhoneNumber") ?? string.Empty,
                Email = GetNullableString(reader, "Email") ?? string.Empty,
                Department = GetNullableString(reader, "Department") ?? string.Empty,
                JobTitle = GetNullableString(reader, "JobTitle") ?? string.Empty,
                EmployeeId = GetNullableString(reader, "EmployeeId"),
                Qualification = GetNullableString(reader, "Qualification") ?? string.Empty,
                AssignedRoomOrWard = GetNullableString(reader, "AssignedRoomOrWard") ?? string.Empty,
                ShiftTiming = GetNullableString(reader, "ShiftTiming") ?? string.Empty,
                HiredByDoctorId = GetNullableInt32(reader, "HiredByDoctorId"),
                HiredByDoctorName = GetNullableString(reader, "HiredByDoctorName"),
                IsActive = GetBoolean(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt")
            };
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
