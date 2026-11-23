using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class AppointmentRepository : AdoNetRepositoryBase
    {
        public AppointmentRepository(DbConnectionHelper dbConnectionHelper) : base(dbConnectionHelper)
        {
        }

        public async Task<int> BookAsync(Appointment appointment)
        {
            const string query = @"
INSERT INTO Appointments
    (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes, IsEmergency, EmergencyDescription, EmergencyAcknowledged, DoctorNotes, VitalsSummary, ESITriageLevel, ApprovedAt, TokenNumber, PaymentStatus, PaymentAmount, PaymentMethod)
OUTPUT INSERTED.AppointmentId
VALUES
    (@PatientId, @DoctorId, @AppointmentDateTime, @Reason, @Status, @Notes, @IsEmergency, @EmergencyDescription, @EmergencyAcknowledged, @DoctorNotes, @VitalsSummary, @ESITriageLevel, @ApprovedAt, @TokenNumber, @PaymentStatus, @PaymentAmount, @PaymentMethod);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", appointment.PatientId);
            command.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
            command.Parameters.AddWithValue("@AppointmentDateTime", appointment.AppointmentDateTime);
            command.Parameters.AddWithValue("@Reason", appointment.Reason.Trim());
            command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(appointment.Status) ? (appointment.IsEmergency ? "Emergency" : "Requested") : appointment.Status.Trim());
            command.Parameters.AddWithValue("@Notes", DbValue(appointment.Notes));
            command.Parameters.AddWithValue("@IsEmergency", appointment.IsEmergency);
            command.Parameters.AddWithValue("@EmergencyDescription", DbValue(appointment.EmergencyDescription));
            command.Parameters.AddWithValue("@EmergencyAcknowledged", appointment.EmergencyAcknowledged);
            command.Parameters.AddWithValue("@DoctorNotes", DbValue(appointment.DoctorNotes));
            command.Parameters.AddWithValue("@VitalsSummary", DbValue(appointment.VitalsSummary));
            command.Parameters.AddWithValue("@ESITriageLevel", DbValue(appointment.ESITriageLevel));
            command.Parameters.AddWithValue("@ApprovedAt", (object?)appointment.ApprovedAt ?? DBNull.Value);
            command.Parameters.AddWithValue("@TokenNumber", (object?)appointment.TokenNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@PaymentStatus", string.IsNullOrWhiteSpace(appointment.PaymentStatus) ? "Pending" : appointment.PaymentStatus.Trim());
            command.Parameters.AddWithValue("@PaymentAmount", appointment.PaymentAmount);
            command.Parameters.AddWithValue("@PaymentMethod", DbValue(appointment.PaymentMethod));

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Appointment appointment)
        {
            const string query = @"
UPDATE Appointments
SET PatientId = @PatientId,
    DoctorId = @DoctorId,
    AppointmentDateTime = @AppointmentDateTime,
    Reason = @Reason,
    Status = @Status,
    Notes = @Notes,
    IsEmergency = @IsEmergency,
    EmergencyDescription = @EmergencyDescription,
    DoctorNotes = @DoctorNotes,
    VitalsSummary = @VitalsSummary,
    ESITriageLevel = @ESITriageLevel,
    TokenNumber = @TokenNumber,
    PaymentStatus = @PaymentStatus,
    PaymentAmount = @PaymentAmount,
    PaymentMethod = @PaymentMethod
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointment.AppointmentId);
            command.Parameters.AddWithValue("@PatientId", appointment.PatientId);
            command.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
            command.Parameters.AddWithValue("@AppointmentDateTime", appointment.AppointmentDateTime);
            command.Parameters.AddWithValue("@Reason", appointment.Reason.Trim());
            command.Parameters.AddWithValue("@Status", appointment.Status.Trim());
            command.Parameters.AddWithValue("@Notes", DbValue(appointment.Notes));
            command.Parameters.AddWithValue("@IsEmergency", appointment.IsEmergency);
            command.Parameters.AddWithValue("@EmergencyDescription", DbValue(appointment.EmergencyDescription));
            command.Parameters.AddWithValue("@DoctorNotes", DbValue(appointment.DoctorNotes));
            command.Parameters.AddWithValue("@VitalsSummary", DbValue(appointment.VitalsSummary));
            command.Parameters.AddWithValue("@ESITriageLevel", DbValue(appointment.ESITriageLevel));
            command.Parameters.AddWithValue("@TokenNumber", (object?)appointment.TokenNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@PaymentStatus", string.IsNullOrWhiteSpace(appointment.PaymentStatus) ? "Pending" : appointment.PaymentStatus.Trim());
            command.Parameters.AddWithValue("@PaymentAmount", appointment.PaymentAmount);
            command.Parameters.AddWithValue("@PaymentMethod", DbValue(appointment.PaymentMethod));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> CancelAsync(int appointmentId)
        {
            const string query = @"
UPDATE Appointments
SET Status = 'Cancelled'
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> ApproveAppointmentAsync(int appointmentId, string? notes = null)
        {
            const string query = @"
UPDATE Appointments
SET Status = 'Approved',
    ApprovedAt = SYSUTCDATETIME(),
    Notes = COALESCE(@Notes, Notes)
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@Notes", DbValue(notes));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> RejectAppointmentAsync(int appointmentId, string? reason = null)
        {
            const string query = @"
UPDATE Appointments
SET Status = 'Rejected',
    Notes = @Notes
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@Notes", DbValue(reason ?? "Declined by Doctor"));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> CompleteAppointmentAsync(int appointmentId, string? doctorNotes)
        {
            const string query = @"
UPDATE Appointments
SET Status = 'Completed',
    DoctorNotes = @DoctorNotes
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@DoctorNotes", DbValue(doctorNotes));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> AcknowledgeEmergencyAsync(int appointmentId)
        {
            const string query = @"
UPDATE Appointments
SET EmergencyAcknowledged = 1,
    Status = 'Emergency - Accepted',
    ApprovedAt = SYSUTCDATETIME()
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateVitalsAndCheckInAsync(int appointmentId, string vitalsSummary)
        {
            const string query = @"
UPDATE Appointments
SET Status = 'Checked-In',
    VitalsSummary = @VitalsSummary
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@VitalsSummary", vitalsSummary.Trim());

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<int> GetNextTokenNumberForDoctorAsync(int doctorId, DateTime date)
        {
            const string query = @"
SELECT ISNULL(MAX(TokenNumber), 0) + 1
FROM Appointments
WHERE DoctorId = @DoctorId AND CAST(AppointmentDateTime AS date) = CAST(@Date AS date);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@Date", date.Date);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> IssueTokenAndCollectFeeAsync(int appointmentId, int tokenNumber, decimal amount, string paymentMethod)
        {
            const string query = @"
UPDATE Appointments
SET TokenNumber = @TokenNumber,
    PaymentStatus = 'Paid',
    PaymentAmount = @Amount,
    PaymentMethod = @PaymentMethod,
    Status = 'Checked-In'
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@TokenNumber", tokenNumber);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@PaymentMethod", paymentMethod.Trim());

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapAppointment(reader);
        }

        public async Task<IReadOnlyList<Appointment>> GetPendingRequestsForDoctorAsync(int doctorId)
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.DoctorId = @DoctorId AND a.Status IN ('Requested', 'Emergency')
ORDER BY a.IsEmergency DESC, a.AppointmentDateTime ASC;";

            List<Appointment> list = new List<Appointment>();
            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapAppointment(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Appointment>> GetScheduleForDoctorAsync(int doctorId)
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.DoctorId = @DoctorId AND a.Status NOT IN ('Cancelled', 'Rejected')
ORDER BY a.IsEmergency DESC, a.AppointmentDateTime ASC;";

            List<Appointment> list = new List<Appointment>();
            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapAppointment(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Appointment>> GetUnacknowledgedEmergenciesForDoctorAsync(int doctorId)
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.DoctorId = @DoctorId 
  AND a.IsEmergency = 1 
  AND a.EmergencyAcknowledged = 0
  AND a.Status NOT IN ('Cancelled', 'Rejected', 'Completed')
ORDER BY a.CreatedAt DESC;";

            List<Appointment> list = new List<Appointment>();
            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapAppointment(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsByPhoneNumberAsync(string phoneNumber)
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE p.PhoneNumber = @PhoneNumber
ORDER BY a.AppointmentDateTime DESC;";

            List<Appointment> list = new List<Appointment>();
            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber.Trim());

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapAppointment(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Appointment>> GetHistoryAsync()
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
ORDER BY a.IsEmergency DESC, a.AppointmentDateTime DESC;";

            List<Appointment> appointments = new List<Appointment>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(MapAppointment(reader));
            }

            return appointments;
        }

        public async Task<IReadOnlyList<Appointment>> GetTodayAppointmentsAsync()
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE CAST(a.AppointmentDateTime AS date) = CAST(GETDATE() AS date)
ORDER BY a.IsEmergency DESC, ISNULL(a.TokenNumber, 999999) ASC, a.AppointmentDateTime ASC;";

            List<Appointment> appointments = new List<Appointment>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(MapAppointment(reader));
            }

            return appointments;
        }

        public async Task<bool> IsDoctorSlotBookedAsync(int doctorId, DateTime slotTime)
        {
            const string query = @"
SELECT COUNT(*)
FROM Appointments
WHERE DoctorId = @DoctorId
  AND Status NOT IN ('Cancelled', 'Rejected')
  AND ABS(DATEDIFF(minute, AppointmentDateTime, @SlotTime)) < 25;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@SlotTime", slotTime);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<IReadOnlyList<DateTime>> GetBookedSlotsForDoctorDateAsync(int doctorId, DateTime date)
        {
            const string query = @"
SELECT AppointmentDateTime
FROM Appointments
WHERE DoctorId = @DoctorId
  AND CAST(AppointmentDateTime AS date) = CAST(@Date AS date)
  AND Status NOT IN ('Cancelled', 'Rejected')
ORDER BY AppointmentDateTime ASC;";

            List<DateTime> bookedSlots = new List<DateTime>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@Date", date.Date);

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bookedSlots.Add(GetDateTime(reader, "AppointmentDateTime"));
            }

            return bookedSlots;
        }

        public async Task<IReadOnlyList<Appointment>> GetAllActiveEmergenciesAsync()
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       a.IsEmergency, a.EmergencyDescription, a.EmergencyAcknowledged, a.DoctorNotes, a.VitalsSummary, a.ESITriageLevel, a.ApprovedAt,
       a.TokenNumber, a.PaymentStatus, a.PaymentAmount, a.PaymentMethod,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       p.PhoneNumber AS PatientPhoneNumber,
       p.MedicalRecordNumber AS PatientMRN,
       d.FullName AS DoctorName,
       d.Specialty AS DoctorSpecialty,
       d.Qualification AS DoctorQualification,
       d.RoomNumber AS DoctorRoomNumber,
       d.ConsultationFee AS DoctorConsultationFee
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.IsEmergency = 1 AND a.Status NOT IN ('Cancelled', 'Rejected', 'Completed')
ORDER BY 
    CASE 
        WHEN a.ESITriageLevel LIKE '%Level 1%' THEN 1
        WHEN a.ESITriageLevel LIKE '%Level 2%' THEN 2
        WHEN a.ESITriageLevel LIKE '%Level 3%' THEN 3
        ELSE 4 
    END ASC,
    a.CreatedAt DESC;";

            List<Appointment> emergencies = new List<Appointment>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                emergencies.Add(MapAppointment(reader));
            }

            return emergencies;
        }

        public async Task<bool> TriageEmergencyAsync(int appointmentId, string esiLevel, string? triageNotes, string? vitals)
        {
            const string query = @"
UPDATE Appointments
SET ESITriageLevel = @ESILevel,
    Notes = COALESCE(@TriageNotes, Notes),
    VitalsSummary = COALESCE(@VitalsSummary, VitalsSummary),
    Status = 'Emergency - Triaged'
WHERE AppointmentId = @AppointmentId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@ESILevel", DbValue(esiLevel));
            command.Parameters.AddWithValue("@TriageNotes", DbValue(triageNotes));
            command.Parameters.AddWithValue("@VitalsSummary", DbValue(vitals));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static Appointment MapAppointment(SqlDataReader reader)
        {
            int tokenOrdinal = reader.GetOrdinal("TokenNumber");
            int paymentAmtOrdinal = reader.GetOrdinal("PaymentAmount");
            int feeOrdinal = reader.GetOrdinal("DoctorConsultationFee");

            return new Appointment
            {
                AppointmentId = GetInt32(reader, "AppointmentId"),
                PatientId = GetInt32(reader, "PatientId"),
                DoctorId = GetInt32(reader, "DoctorId"),
                AppointmentDateTime = GetDateTime(reader, "AppointmentDateTime"),
                Reason = GetNullableString(reader, "Reason") ?? string.Empty,
                Status = GetNullableString(reader, "Status") ?? string.Empty,
                Notes = GetNullableString(reader, "Notes"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                IsEmergency = GetBoolean(reader, "IsEmergency"),
                EmergencyDescription = GetNullableString(reader, "EmergencyDescription"),
                EmergencyAcknowledged = GetBoolean(reader, "EmergencyAcknowledged"),
                DoctorNotes = GetNullableString(reader, "DoctorNotes"),
                VitalsSummary = GetNullableString(reader, "VitalsSummary"),
                ESITriageLevel = GetNullableString(reader, "ESITriageLevel"),
                ApprovedAt = GetNullableDateTime(reader, "ApprovedAt"),
                TokenNumber = reader.IsDBNull(tokenOrdinal) ? null : Convert.ToInt32(reader.GetValue(tokenOrdinal)),
                PaymentStatus = GetNullableString(reader, "PaymentStatus") ?? "Pending",
                PaymentAmount = reader.IsDBNull(paymentAmtOrdinal) ? 0 : Convert.ToDecimal(reader.GetValue(paymentAmtOrdinal)),
                PaymentMethod = GetNullableString(reader, "PaymentMethod"),
                PatientName = GetNullableString(reader, "PatientName"),
                PatientPhoneNumber = GetNullableString(reader, "PatientPhoneNumber"),
                PatientMRN = GetNullableString(reader, "PatientMRN"),
                DoctorName = GetNullableString(reader, "DoctorName"),
                DoctorSpecialty = GetNullableString(reader, "DoctorSpecialty"),
                DoctorQualification = GetNullableString(reader, "DoctorQualification"),
                DoctorRoomNumber = GetNullableString(reader, "DoctorRoomNumber"),
                DoctorConsultationFee = reader.IsDBNull(feeOrdinal) ? 0 : Convert.ToDecimal(reader.GetValue(feeOrdinal))
            };
        }
    }
}
