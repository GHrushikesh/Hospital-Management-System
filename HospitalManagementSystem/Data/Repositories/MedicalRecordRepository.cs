using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class MedicalRecordRepository : AdoNetRepositoryBase
    {
        public MedicalRecordRepository(DbConnectionHelper dbConnectionHelper)
            : base(dbConnectionHelper)
        {
        }

        public async Task<int> AddAsync(MedicalRecord record)
        {
            const string query = @"
INSERT INTO MedicalRecords
(
    PatientId,
    DoctorId,
    AppointmentId,
    Diagnosis,
    Treatment,
    Prescription,
    Notes,
    RecordDate
)
OUTPUT INSERTED.MedicalRecordId
VALUES
(
    @PatientId,
    @DoctorId,
    @AppointmentId,
    @Diagnosis,
    @Treatment,
    @Prescription,
    @Notes,
    @RecordDate
);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PatientId", record.PatientId);
            command.Parameters.AddWithValue("@DoctorId", record.DoctorId);
            command.Parameters.AddWithValue(
                "@AppointmentId",
                (object?)record.AppointmentId ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@Diagnosis",
                record.Diagnosis.Trim());
            command.Parameters.AddWithValue(
                "@Treatment",
                DbValue(record.Treatment));
            command.Parameters.AddWithValue(
                "@Prescription",
                DbValue(record.Prescription));
            command.Parameters.AddWithValue(
                "@Notes",
                DbValue(record.Notes));
            command.Parameters.AddWithValue(
                "@RecordDate",
                record.RecordDate == default
                    ? DateTime.UtcNow
                    : record.RecordDate);

            object? result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(MedicalRecord record)
        {
            const string query = @"
UPDATE MedicalRecords
SET
    PatientId = @PatientId,
    DoctorId = @DoctorId,
    AppointmentId = @AppointmentId,
    Diagnosis = @Diagnosis,
    Treatment = @Treatment,
    Prescription = @Prescription,
    Notes = @Notes,
    RecordDate = @RecordDate
WHERE MedicalRecordId = @MedicalRecordId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@MedicalRecordId",
                record.MedicalRecordId);
            command.Parameters.AddWithValue(
                "@PatientId",
                record.PatientId);
            command.Parameters.AddWithValue(
                "@DoctorId",
                record.DoctorId);
            command.Parameters.AddWithValue(
                "@AppointmentId",
                (object?)record.AppointmentId ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@Diagnosis",
                record.Diagnosis.Trim());
            command.Parameters.AddWithValue(
                "@Treatment",
                DbValue(record.Treatment));
            command.Parameters.AddWithValue(
                "@Prescription",
                DbValue(record.Prescription));
            command.Parameters.AddWithValue(
                "@Notes",
                DbValue(record.Notes));
            command.Parameters.AddWithValue(
                "@RecordDate",
                record.RecordDate);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int medicalRecordId)
        {
            const string query = @"
DELETE FROM MedicalRecords
WHERE MedicalRecordId = @MedicalRecordId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@MedicalRecordId",
                medicalRecordId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<MedicalRecord?> GetByIdAsync(
            int medicalRecordId)
        {
            const string query = @"
SELECT
    m.MedicalRecordId,
    m.PatientId,
    m.DoctorId,
    m.AppointmentId,
    m.Diagnosis,
    m.Treatment,
    m.Prescription,
    m.Notes,
    m.RecordDate,
    m.CreatedAt,
    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
    d.FullName AS DoctorName
FROM MedicalRecords m
INNER JOIN Patients p
    ON m.PatientId = p.PatientId
INNER JOIN Doctors d
    ON m.DoctorId = d.DoctorId
WHERE m.MedicalRecordId = @MedicalRecordId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@MedicalRecordId",
                medicalRecordId);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapMedicalRecord(reader);
        }

        public async Task<IReadOnlyList<MedicalRecord>> GetAllAsync()
        {
            const string query = @"
SELECT
    m.MedicalRecordId,
    m.PatientId,
    m.DoctorId,
    m.AppointmentId,
    m.Diagnosis,
    m.Treatment,
    m.Prescription,
    m.Notes,
    m.RecordDate,
    m.CreatedAt,
    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
    d.FullName AS DoctorName
FROM MedicalRecords m
INNER JOIN Patients p
    ON m.PatientId = p.PatientId
INNER JOIN Doctors d
    ON m.DoctorId = d.DoctorId
ORDER BY m.RecordDate DESC;";

            List<MedicalRecord> records = new List<MedicalRecord>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(MapMedicalRecord(reader));
            }

            return records;
        }

        public async Task<IReadOnlyList<MedicalRecord>> GetByPatientAsync(
            int patientId)
        {
            const string query = @"
SELECT
    m.MedicalRecordId,
    m.PatientId,
    m.DoctorId,
    m.AppointmentId,
    m.Diagnosis,
    m.Treatment,
    m.Prescription,
    m.Notes,
    m.RecordDate,
    m.CreatedAt,
    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
    d.FullName AS DoctorName
FROM MedicalRecords m
INNER JOIN Patients p
    ON m.PatientId = p.PatientId
INNER JOIN Doctors d
    ON m.DoctorId = d.DoctorId
WHERE m.PatientId = @PatientId
ORDER BY m.RecordDate DESC;";

            List<MedicalRecord> records = new List<MedicalRecord>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PatientId", patientId);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(MapMedicalRecord(reader));
            }

            return records;
        }

        public async Task<IReadOnlyList<MedicalRecord>> SearchAsync(
            string searchTerm)
        {
            const string query = @"
SELECT
    m.MedicalRecordId,
    m.PatientId,
    m.DoctorId,
    m.AppointmentId,
    m.Diagnosis,
    m.Treatment,
    m.Prescription,
    m.Notes,
    m.RecordDate,
    m.CreatedAt,
    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
    d.FullName AS DoctorName
FROM MedicalRecords m
INNER JOIN Patients p
    ON m.PatientId = p.PatientId
INNER JOIN Doctors d
    ON m.DoctorId = d.DoctorId
WHERE CONCAT(p.FirstName, ' ', p.LastName) LIKE @SearchTerm
   OR d.FullName LIKE @SearchTerm
   OR m.Diagnosis LIKE @SearchTerm
   OR m.Treatment LIKE @SearchTerm
   OR m.Prescription LIKE @SearchTerm
ORDER BY m.RecordDate DESC;";

            List<MedicalRecord> records = new List<MedicalRecord>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@SearchTerm",
                $"%{searchTerm.Trim()}%");

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(MapMedicalRecord(reader));
            }

            return records;
        }

        private static MedicalRecord MapMedicalRecord(
            SqlDataReader reader)
        {
            return new MedicalRecord
            {
                MedicalRecordId =
                    GetInt32(reader, "MedicalRecordId"),

                PatientId =
                    GetInt32(reader, "PatientId"),

                DoctorId =
                    GetInt32(reader, "DoctorId"),

                AppointmentId =
                    reader.IsDBNull(
                        reader.GetOrdinal("AppointmentId"))
                        ? null
                        : (int?)reader.GetInt32(
                            reader.GetOrdinal("AppointmentId")),

                Diagnosis =
                    GetNullableString(reader, "Diagnosis")
                    ?? string.Empty,

                Treatment =
                    GetNullableString(reader, "Treatment"),

                Prescription =
                    GetNullableString(reader, "Prescription"),

                Notes =
                    GetNullableString(reader, "Notes"),

                RecordDate =
                    GetDateTime(reader, "RecordDate"),

                CreatedAt =
                    GetDateTime(reader, "CreatedAt"),

                PatientName =
                    GetNullableString(reader, "PatientName"),

                DoctorName =
                    GetNullableString(reader, "DoctorName")
            };
        }
    }
}