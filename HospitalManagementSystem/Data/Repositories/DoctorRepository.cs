using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

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
    (FullName, Specialty, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive)
OUTPUT INSERTED.DoctorId
VALUES
    (@FullName, @Specialty, @PhoneNumber, @Email, @RoomNumber, @Availability, @ConsultationFee, @IsActive);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FullName", doctor.FullName.Trim());
            command.Parameters.AddWithValue("@Specialty", doctor.Specialty.Trim());
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
SELECT DoctorId, FullName, Specialty, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive, CreatedAt
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
SELECT DoctorId, FullName, Specialty, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive, CreatedAt
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
SELECT DoctorId, FullName, Specialty, PhoneNumber, Email, RoomNumber, Availability, ConsultationFee, IsActive, CreatedAt
FROM Doctors
WHERE FullName LIKE @SearchTerm
   OR Specialty LIKE @SearchTerm
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
                PhoneNumber = GetNullableString(reader, "PhoneNumber"),
                Email = GetNullableString(reader, "Email"),
                RoomNumber = GetNullableString(reader, "RoomNumber"),
                Availability = GetNullableString(reader, "Availability"),
                ConsultationFee = GetDecimal(reader, "ConsultationFee"),
                IsActive = GetBoolean(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt")
            };
        }
    }
}