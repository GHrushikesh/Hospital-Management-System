using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class PatientRepository : AdoNetRepositoryBase
    {
        public PatientRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<int> AddAsync(Patient patient)
        {
            const string query = @"
INSERT INTO Patients
    (FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup, EmergencyContactName, EmergencyContactNumber)
OUTPUT INSERTED.PatientId
VALUES
    (@FirstName, @LastName, @DateOfBirth, @Gender, @PhoneNumber, @Email, @Address, @BloodGroup, @EmergencyContactName, @EmergencyContactNumber);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", patient.FirstName.Trim());
            command.Parameters.AddWithValue("@LastName", patient.LastName.Trim());
            command.Parameters.AddWithValue("@DateOfBirth", (object?)patient.DateOfBirth ?? DBNull.Value);
            command.Parameters.AddWithValue("@Gender", patient.Gender.Trim());
            command.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber.Trim());
            command.Parameters.AddWithValue("@Email", DbValue(patient.Email));
            command.Parameters.AddWithValue("@Address", DbValue(patient.Address));
            command.Parameters.AddWithValue("@BloodGroup", DbValue(patient.BloodGroup));
            command.Parameters.AddWithValue("@EmergencyContactName", DbValue(patient.EmergencyContactName));
            command.Parameters.AddWithValue("@EmergencyContactNumber", DbValue(patient.EmergencyContactNumber));

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Patient patient)
        {
            const string query = @"
UPDATE Patients
SET FirstName = @FirstName,
    LastName = @LastName,
    DateOfBirth = @DateOfBirth,
    Gender = @Gender,
    PhoneNumber = @PhoneNumber,
    Email = @Email,
    Address = @Address,
    BloodGroup = @BloodGroup,
    EmergencyContactName = @EmergencyContactName,
    EmergencyContactNumber = @EmergencyContactNumber
WHERE PatientId = @PatientId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", patient.PatientId);
            command.Parameters.AddWithValue("@FirstName", patient.FirstName.Trim());
            command.Parameters.AddWithValue("@LastName", patient.LastName.Trim());
            command.Parameters.AddWithValue("@DateOfBirth", (object?)patient.DateOfBirth ?? DBNull.Value);
            command.Parameters.AddWithValue("@Gender", patient.Gender.Trim());
            command.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber.Trim());
            command.Parameters.AddWithValue("@Email", DbValue(patient.Email));
            command.Parameters.AddWithValue("@Address", DbValue(patient.Address));
            command.Parameters.AddWithValue("@BloodGroup", DbValue(patient.BloodGroup));
            command.Parameters.AddWithValue("@EmergencyContactName", DbValue(patient.EmergencyContactName));
            command.Parameters.AddWithValue("@EmergencyContactNumber", DbValue(patient.EmergencyContactNumber));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int patientId)
        {
            const string query = @"DELETE FROM Patients WHERE PatientId = @PatientId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Patient?> GetByIdAsync(int patientId)
        {
            const string query = @"
SELECT PatientId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup,
       EmergencyContactName, EmergencyContactNumber, CreatedAt
FROM Patients
WHERE PatientId = @PatientId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapPatient(reader);
        }

        public async Task<IReadOnlyList<Patient>> GetAllAsync()
        {
            const string query = @"
SELECT PatientId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup,
       EmergencyContactName, EmergencyContactNumber, CreatedAt
FROM Patients
ORDER BY CreatedAt DESC;";

            List<Patient> patients = new List<Patient>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                patients.Add(MapPatient(reader));
            }

            return patients;
        }

        public async Task<IReadOnlyList<Patient>> SearchAsync(string searchTerm)
        {
            const string query = @"
SELECT PatientId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, BloodGroup,
       EmergencyContactName, EmergencyContactNumber, CreatedAt
FROM Patients
WHERE FirstName LIKE @SearchTerm
   OR LastName LIKE @SearchTerm
   OR PhoneNumber LIKE @SearchTerm
   OR Email LIKE @SearchTerm
ORDER BY CreatedAt DESC;";

            List<Patient> patients = new List<Patient>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm.Trim()}%");

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                patients.Add(MapPatient(reader));
            }

            return patients;
        }

        private static Patient MapPatient(SqlDataReader reader)
        {
            return new Patient
            {
                PatientId = GetInt32(reader, "PatientId"),
                FirstName = GetNullableString(reader, "FirstName") ?? string.Empty,
                LastName = GetNullableString(reader, "LastName") ?? string.Empty,
                DateOfBirth = GetNullableDateTime(reader, "DateOfBirth"),
                Gender = GetNullableString(reader, "Gender") ?? string.Empty,
                PhoneNumber = GetNullableString(reader, "PhoneNumber") ?? string.Empty,
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                BloodGroup = GetNullableString(reader, "BloodGroup"),
                EmergencyContactName = GetNullableString(reader, "EmergencyContactName"),
                EmergencyContactNumber = GetNullableString(reader, "EmergencyContactNumber"),
                CreatedAt = GetDateTime(reader, "CreatedAt")
            };
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                const string query = @"SELECT COUNT(1) FROM Patients;";
                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync();

                await using SqlCommand command = new SqlCommand(query, connection);
                object? result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error fetching Patient count: {ex.Message}");
                return 0;
            }
        }
    }
}