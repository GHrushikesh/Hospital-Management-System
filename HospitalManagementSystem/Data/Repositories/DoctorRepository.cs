using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagementSystem.Data.Repositories
{
    public class DoctorRepository : AdoNetRepositoryBase
    {
        public DoctorRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<int> AddAsync(Doctor doctor)
        {
            const string query = @"
INSERT INTO Doctors
    (FullName, Specialty, Qualification, Username, PasswordHash, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
OUTPUT INSERTED.DoctorId
VALUES
    (@FullName, @Specialty, @Qualification, @Username, @PasswordHash, @PhoneNumber, @Email, @RoomNumber, @Availability, @ConsultationFee, @IsActive);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FullName", doctor.FullName.Trim());
            command.Parameters.AddWithValue("@Specialty", doctor.Specialty.Trim());
            command.Parameters.AddWithValue("@Qualification", string.IsNullOrWhiteSpace(doctor.Qualification) ? "MBBS, MD" : doctor.Qualification.Trim());
            command.Parameters.AddWithValue("@Username", doctor.Username.Trim());
            command.Parameters.AddWithValue("@PasswordHash", ComputeHash(doctor.Password ?? throw new InvalidOperationException("Doctor password is required.")));
            command.Parameters.AddWithValue("@PhoneNumber", DbValue(doctor.PhoneNumber));
            command.Parameters.AddWithValue("@Email", DbValue(doctor.Email));
            command.Parameters.AddWithValue("@RoomNumber", DbValue(doctor.RoomNumber));
            command.Parameters.AddWithValue("@Availability", DbValue(doctor.Availability));
            command.Parameters.AddWithValue("@ConsultationFee", doctor.ConsultationFee);
            command.Parameters.AddWithValue("@IsActive", doctor.IsActive);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Doctor doctor)
        {
            const string query = @"
UPDATE Doctors
SET FullName = @FullName,
    Specialty = @Specialty,
    Qualification = @Qualification,
    Username = @Username,
    PasswordHash = COALESCE(@PasswordHash, PasswordHash),
    PhoneNumber = @PhoneNumber,
    Email = @Email,
    RoomNumber = @RoomNumber,
    Availability = @Availability,
    ConsultationFee = @ConsultationFee,
    IsActive = @IsActive
WHERE DoctorId = @DoctorId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctor.DoctorId);
            command.Parameters.AddWithValue("@FullName", doctor.FullName.Trim());
            command.Parameters.AddWithValue("@Specialty", doctor.Specialty.Trim());
            command.Parameters.AddWithValue("@Qualification", string.IsNullOrWhiteSpace(doctor.Qualification) ? "MBBS, MD" : doctor.Qualification.Trim());
            command.Parameters.AddWithValue("@Username", doctor.Username.Trim());
            command.Parameters.AddWithValue("@PasswordHash", string.IsNullOrWhiteSpace(doctor.Password) ? DBNull.Value : ComputeHash(doctor.Password));
            command.Parameters.AddWithValue("@PhoneNumber", DbValue(doctor.PhoneNumber));
            command.Parameters.AddWithValue("@Email", DbValue(doctor.Email));
            command.Parameters.AddWithValue("@RoomNumber", DbValue(doctor.RoomNumber));
            command.Parameters.AddWithValue("@Availability", DbValue(doctor.Availability));
            command.Parameters.AddWithValue("@ConsultationFee", doctor.ConsultationFee);
            command.Parameters.AddWithValue("@IsActive", doctor.IsActive);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int doctorId)
        {
            const string query = @"DELETE FROM Doctors WHERE DoctorId = @DoctorId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Doctor?> GetByIdAsync(int doctorId)
        {
            const string query = @"
SELECT DoctorId, FullName, Specialty, Qualification, Username, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive, CreatedAt
FROM Doctors
WHERE DoctorId = @DoctorId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapDoctor(reader);
        }

        public async Task<IReadOnlyList<Doctor>> GetAllAsync()
        {
            const string query = @"
SELECT DoctorId, FullName, Specialty, Qualification, Username, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive, CreatedAt
FROM Doctors
ORDER BY FullName;";

            List<Doctor> doctors = new List<Doctor>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                doctors.Add(MapDoctor(reader));
            }

            return doctors;
        }

        public async Task<IReadOnlyList<Doctor>> SearchAsync(string searchTerm)
        {
            const string query = @"
SELECT DoctorId, FullName, Specialty, Qualification, Username, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive, CreatedAt
FROM Doctors
WHERE FullName LIKE @SearchTerm
   OR Specialty LIKE @SearchTerm
   OR Qualification LIKE @SearchTerm
   OR PhoneNumber LIKE @SearchTerm
ORDER BY FullName;";

            List<Doctor> doctors = new List<Doctor>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm.Trim()}%");

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                doctors.Add(MapDoctor(reader));
            }

            return doctors;
        }

        private static Doctor MapDoctor(SqlDataReader reader)
        {
            return new Doctor
            {
                DoctorId = GetInt32(reader, "DoctorId"),
                FullName = GetNullableString(reader, "FullName") ?? string.Empty,
                Specialty = GetNullableString(reader, "Specialty") ?? string.Empty,
                Qualification = GetNullableString(reader, "Qualification") ?? "MBBS, MD",
                Username = GetNullableString(reader, "Username") ?? string.Empty,
                PhoneNumber = GetNullableString(reader, "PhoneNumber"),
                Email = GetNullableString(reader, "Email"),
                RoomNumber = GetNullableString(reader, "RoomNumber"),
                Availability = GetNullableString(reader, "Availability"),
                ConsultationFee = GetDecimal(reader, "ConsultationFee"),
                IsActive = GetBoolean(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt")
            };
        }

        public async Task<Doctor?> AuthenticateAsync(string username, string password)
        {
            const string query = @"
SELECT DoctorId, FullName, Specialty, Qualification, Username, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive, CreatedAt, PasswordHash
FROM Doctors
WHERE Username = @Username AND IsActive = 1;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Username", username.Trim());
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var storedHash = GetNullableString(reader, "PasswordHash") ?? string.Empty;
            var submittedHash = ComputeHash(password);

            if (!string.Equals(storedHash, submittedHash, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(storedHash, password.Trim(), StringComparison.Ordinal))
            {
                return null;
            }

            return MapDoctor(reader);
        }

        public static string ComputeHash(string input)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input.Trim()))).ToLowerInvariant();
        }
    }
}
