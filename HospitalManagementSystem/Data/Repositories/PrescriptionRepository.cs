using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class PrescriptionRepository : AdoNetRepositoryBase
    {
        public PrescriptionRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<int> AddAsync(Prescription prescription)
        {
            const string query = @"
INSERT INTO Prescriptions (PatientId, DoctorId, AppointmentId, MedicineName, Dosage, Frequency, Duration, Instructions, PrescribedAt)
OUTPUT INSERTED.PrescriptionId
VALUES (@PatientId, @DoctorId, @AppointmentId, @MedicineName, @Dosage, @Frequency, @Duration, @Instructions, SYSUTCDATETIME());";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", prescription.PatientId);
            command.Parameters.AddWithValue("@DoctorId", prescription.DoctorId);
            command.Parameters.AddWithValue("@AppointmentId", (object?)prescription.AppointmentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@MedicineName", prescription.MedicineName.Trim());
            command.Parameters.AddWithValue("@Dosage", prescription.Dosage.Trim());
            command.Parameters.AddWithValue("@Frequency", prescription.Frequency.Trim());
            command.Parameters.AddWithValue("@Duration", prescription.Duration.Trim());
            command.Parameters.AddWithValue("@Instructions", DbValue(prescription.Instructions));

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IReadOnlyList<Prescription>> GetByPatientIdAsync(int patientId)
        {
            const string query = @"
SELECT pr.PrescriptionId, pr.PatientId, pr.DoctorId, pr.AppointmentId, pr.MedicineName, pr.Dosage,
       pr.Frequency, pr.Duration, pr.Instructions, pr.PrescribedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty
FROM Prescriptions pr
INNER JOIN Patients p ON pr.PatientId = p.PatientId
INNER JOIN Doctors d ON pr.DoctorId = d.DoctorId
WHERE pr.PatientId = @PatientId
ORDER BY pr.PrescribedAt DESC;";

            List<Prescription> list = new List<Prescription>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapPrescription(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Prescription>> GetByAppointmentIdAsync(int appointmentId)
        {
            const string query = @"
SELECT pr.PrescriptionId, pr.PatientId, pr.DoctorId, pr.AppointmentId, pr.MedicineName, pr.Dosage,
       pr.Frequency, pr.Duration, pr.Instructions, pr.PrescribedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty
FROM Prescriptions pr
INNER JOIN Patients p ON pr.PatientId = p.PatientId
INNER JOIN Doctors d ON pr.DoctorId = d.DoctorId
WHERE pr.AppointmentId = @AppointmentId
ORDER BY pr.PrescribedAt DESC;";

            List<Prescription> list = new List<Prescription>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapPrescription(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Prescription>> GetRecentPrescriptionsAsync(int limit = 15)
        {
            string query = $@"
SELECT TOP ({limit}) pr.PrescriptionId, pr.PatientId, pr.DoctorId, pr.AppointmentId, pr.MedicineName, pr.Dosage,
       pr.Frequency, pr.Duration, pr.Instructions, pr.PrescribedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty
FROM Prescriptions pr
INNER JOIN Patients p ON pr.PatientId = p.PatientId
INNER JOIN Doctors d ON pr.DoctorId = d.DoctorId
ORDER BY pr.PrescribedAt DESC;";

            List<Prescription> list = new List<Prescription>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapPrescription(reader));
            }

            return list;
        }

        private static Prescription MapPrescription(SqlDataReader reader)
        {
            return new Prescription
            {
                PrescriptionId = GetInt32(reader, "PrescriptionId"),
                PatientId = GetInt32(reader, "PatientId"),
                DoctorId = GetInt32(reader, "DoctorId"),
                AppointmentId = GetNullableInt32(reader, "AppointmentId"),
                MedicineName = GetNullableString(reader, "MedicineName") ?? string.Empty,
                Dosage = GetNullableString(reader, "Dosage") ?? string.Empty,
                Frequency = GetNullableString(reader, "Frequency") ?? string.Empty,
                Duration = GetNullableString(reader, "Duration") ?? string.Empty,
                Instructions = GetNullableString(reader, "Instructions"),
                PrescribedAt = GetDateTime(reader, "PrescribedAt"),
                PatientName = GetNullableString(reader, "PatientName"),
                PatientMRN = GetNullableString(reader, "PatientMRN"),
                DoctorName = GetNullableString(reader, "DoctorName"),
                DoctorSpecialty = GetNullableString(reader, "DoctorSpecialty")
            };
        }
    }
}
