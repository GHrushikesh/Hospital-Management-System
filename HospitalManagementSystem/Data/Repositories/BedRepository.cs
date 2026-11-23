using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class BedRepository : AdoNetRepositoryBase
    {
        public BedRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<IReadOnlyList<HospitalBed>> GetAllAsync()
        {
            const string query = @"
SELECT b.BedId, b.BedNumber, b.WardName, b.BedType, b.Floor, b.Status,
       b.PatientId, b.AdmittedDoctorId, b.AdmissionNotes, b.AdmittedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       p.PhoneNumber AS PatientPhoneNumber,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty
FROM HospitalBeds b
LEFT JOIN Patients p ON b.PatientId = p.PatientId
LEFT JOIN Doctors d ON b.AdmittedDoctorId = d.DoctorId
ORDER BY b.WardName, b.BedNumber;";

            List<HospitalBed> beds = new List<HospitalBed>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                beds.Add(MapHospitalBed(reader));
            }

            return beds;
        }

        public async Task<HospitalBed?> GetByIdAsync(int bedId)
        {
            const string query = @"
SELECT TOP 1 b.BedId, b.BedNumber, b.WardName, b.BedType, b.Floor, b.Status,
       b.PatientId, b.AdmittedDoctorId, b.AdmissionNotes, b.AdmittedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       p.PhoneNumber AS PatientPhoneNumber,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty
FROM HospitalBeds b
LEFT JOIN Patients p ON b.PatientId = p.PatientId
LEFT JOIN Doctors d ON b.AdmittedDoctorId = d.DoctorId
WHERE b.BedId = @BedId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BedId", bedId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapHospitalBed(reader);
            }

            return null;
        }

        public async Task<HospitalBed?> GetActiveBedByPatientIdAsync(int patientId)
        {
            const string query = @"
SELECT TOP 1 b.BedId, b.BedNumber, b.WardName, b.BedType, b.Floor, b.Status,
       b.PatientId, b.AdmittedDoctorId, b.AdmissionNotes, b.AdmittedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       p.PhoneNumber AS PatientPhoneNumber,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty
FROM HospitalBeds b
LEFT JOIN Patients p ON b.PatientId = p.PatientId
LEFT JOIN Doctors d ON b.AdmittedDoctorId = d.DoctorId
WHERE b.PatientId = @PatientId AND b.Status = 'Occupied';";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapHospitalBed(reader);
            }

            return null;
        }

        public async Task<bool> AdmitPatientAsync(int bedId, int patientId, int doctorId, string admissionNotes)
        {
            // 1. Concurrency and safety check: Is the patient already occupying a bed?
            var existingBed = await GetActiveBedByPatientIdAsync(patientId);
            if (existingBed != null)
            {
                return false;
            }

            var targetBed = await GetByIdAsync(bedId);
            if (targetBed == null || targetBed.Status != "Vacant")
            {
                return false;
            }

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = connection.BeginTransaction();

            try
            {
                // Update bed status
                const string updateBedQuery = @"
UPDATE HospitalBeds
SET Status = 'Occupied',
    PatientId = @PatientId,
    AdmittedDoctorId = @DoctorId,
    AdmissionNotes = @AdmissionNotes,
    AdmittedAt = SYSUTCDATETIME()
WHERE BedId = @BedId AND Status = 'Vacant';";

                await using var updateCmd = new SqlCommand(updateBedQuery, connection, transaction);
                updateCmd.Parameters.AddWithValue("@BedId", bedId);
                updateCmd.Parameters.AddWithValue("@PatientId", patientId);
                updateCmd.Parameters.AddWithValue("@DoctorId", doctorId);
                updateCmd.Parameters.AddWithValue("@AdmissionNotes", DbValue(admissionNotes));

                int rows = await updateCmd.ExecuteNonQueryAsync();
                if (rows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Insert into permanent BedAdmissions historical log
                const string insertLogQuery = @"
INSERT INTO BedAdmissions (BedId, BedNumber, WardName, PatientId, AdmittedDoctorId, AdmissionNotes, AdmittedAt, Status)
VALUES (@BedId, @BedNumber, @WardName, @PatientId, @DoctorId, @AdmissionNotes, SYSUTCDATETIME(), 'Active');";

                await using var logCmd = new SqlCommand(insertLogQuery, connection, transaction);
                logCmd.Parameters.AddWithValue("@BedId", bedId);
                logCmd.Parameters.AddWithValue("@BedNumber", targetBed.BedNumber);
                logCmd.Parameters.AddWithValue("@WardName", targetBed.WardName);
                logCmd.Parameters.AddWithValue("@PatientId", patientId);
                logCmd.Parameters.AddWithValue("@DoctorId", doctorId);
                logCmd.Parameters.AddWithValue("@AdmissionNotes", DbValue(admissionNotes));

                await logCmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DischargePatientAsync(int bedId, string? dischargeSummary, string? dischargedByStaff)
        {
            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Close out the active historical admission record
                const string updateLogQuery = @"
UPDATE BedAdmissions
SET DischargedAt = SYSUTCDATETIME(),
    DischargeSummary = @DischargeSummary,
    DischargedByStaff = @DischargedByStaff,
    Status = 'Discharged'
WHERE BedId = @BedId AND Status = 'Active';";

                await using var logCmd = new SqlCommand(updateLogQuery, connection, transaction);
                logCmd.Parameters.AddWithValue("@BedId", bedId);
                logCmd.Parameters.AddWithValue("@DischargeSummary", DbValue(dischargeSummary ?? "Physician approved discharge. Vitals stable."));
                logCmd.Parameters.AddWithValue("@DischargedByStaff", DbValue(dischargedByStaff ?? "Ward Charge Nurse"));
                await logCmd.ExecuteNonQueryAsync();

                // 2. Mark HospitalBed as vacant and clear live fields
                const string updateBedQuery = @"
UPDATE HospitalBeds
SET Status = 'Vacant',
    PatientId = NULL,
    AdmittedDoctorId = NULL,
    AdmissionNotes = NULL,
    AdmittedAt = NULL
WHERE BedId = @BedId;";

                await using var bedCmd = new SqlCommand(updateBedQuery, connection, transaction);
                bedCmd.Parameters.AddWithValue("@BedId", bedId);
                int rows = await bedCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return rows > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IReadOnlyList<BedAdmission>> GetAdmissionHistoryAsync(int count = 50)
        {
            string query = $@"
SELECT TOP ({count})
       ba.AdmissionId, ba.BedId, ba.BedNumber, ba.WardName, ba.PatientId, ba.AdmittedDoctorId,
       ba.AdmissionNotes, ba.AdmittedAt, ba.DischargedAt, ba.DischargeSummary, ba.DischargedByStaff, ba.Status,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       p.PhoneNumber AS PatientPhoneNumber,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty
FROM BedAdmissions ba
INNER JOIN Patients p ON ba.PatientId = p.PatientId
INNER JOIN Doctors d ON ba.AdmittedDoctorId = d.DoctorId
ORDER BY ba.AdmittedAt DESC;";

            List<BedAdmission> history = new List<BedAdmission>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                history.Add(new BedAdmission
                {
                    AdmissionId = GetInt32(reader, "AdmissionId"),
                    BedId = GetInt32(reader, "BedId"),
                    BedNumber = GetNullableString(reader, "BedNumber") ?? string.Empty,
                    WardName = GetNullableString(reader, "WardName") ?? string.Empty,
                    PatientId = GetInt32(reader, "PatientId"),
                    AdmittedDoctorId = GetInt32(reader, "AdmittedDoctorId"),
                    AdmissionNotes = GetNullableString(reader, "AdmissionNotes"),
                    AdmittedAt = GetDateTime(reader, "AdmittedAt"),
                    DischargedAt = GetNullableDateTime(reader, "DischargedAt"),
                    DischargeSummary = GetNullableString(reader, "DischargeSummary"),
                    DischargedByStaff = GetNullableString(reader, "DischargedByStaff"),
                    Status = GetNullableString(reader, "Status") ?? "Active",
                    PatientName = GetNullableString(reader, "PatientName"),
                    PatientMRN = GetNullableString(reader, "PatientMRN"),
                    PatientPhoneNumber = GetNullableString(reader, "PatientPhoneNumber"),
                    DoctorName = GetNullableString(reader, "DoctorName"),
                    DoctorSpecialty = GetNullableString(reader, "DoctorSpecialty")
                });
            }

            return history;
        }

        public async Task<(int Total, int Occupied, int Vacant)> GetOccupancySummaryAsync()
        {
            const string query = @"
SELECT 
    COUNT(*) AS TotalBeds,
    SUM(CASE WHEN Status = 'Occupied' THEN 1 ELSE 0 END) AS OccupiedBeds,
    SUM(CASE WHEN Status = 'Vacant' THEN 1 ELSE 0 END) AS VacantBeds
FROM HospitalBeds;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int total = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                int occupied = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                int vacant = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                return (total, occupied, vacant);
            }

            return (0, 0, 0);
        }

        private static HospitalBed MapHospitalBed(SqlDataReader reader)
        {
            return new HospitalBed
            {
                BedId = GetInt32(reader, "BedId"),
                BedNumber = GetNullableString(reader, "BedNumber") ?? string.Empty,
                WardName = GetNullableString(reader, "WardName") ?? string.Empty,
                BedType = GetNullableString(reader, "BedType") ?? string.Empty,
                Floor = GetNullableString(reader, "Floor") ?? string.Empty,
                Status = GetNullableString(reader, "Status") ?? "Vacant",
                PatientId = GetNullableInt32(reader, "PatientId"),
                AdmittedDoctorId = GetNullableInt32(reader, "AdmittedDoctorId"),
                AdmissionNotes = GetNullableString(reader, "AdmissionNotes"),
                AdmittedAt = GetNullableDateTime(reader, "AdmittedAt"),
                PatientName = GetNullableString(reader, "PatientName"),
                PatientMRN = GetNullableString(reader, "PatientMRN"),
                PatientPhoneNumber = GetNullableString(reader, "PatientPhoneNumber"),
                DoctorName = GetNullableString(reader, "DoctorName"),
                DoctorSpecialty = GetNullableString(reader, "DoctorSpecialty")
            };
        }
    }
}
