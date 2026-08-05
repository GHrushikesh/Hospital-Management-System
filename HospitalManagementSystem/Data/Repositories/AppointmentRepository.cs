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
    (PatientId, DoctorId, AppointmentDateTime, Reason, Status, Notes)
OUTPUT INSERTED.AppointmentId
VALUES
    (@PatientId, @DoctorId, @AppointmentDateTime, @Reason, @Status, @Notes);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", appointment.PatientId);
            command.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
            command.Parameters.AddWithValue("@AppointmentDateTime", appointment.AppointmentDateTime);
            command.Parameters.AddWithValue("@Reason", appointment.Reason.Trim());
            command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(appointment.Status) ? "Scheduled" : appointment.Status.Trim());
            command.Parameters.AddWithValue("@Notes", DbValue(appointment.Notes));

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
    Notes = @Notes
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

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       d.FullName AS DoctorName
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

        public async Task<IReadOnlyList<Appointment>> GetHistoryAsync()
        {
            const string query = @"
SELECT a.AppointmentId, a.PatientId, a.DoctorId, a.AppointmentDateTime, a.Reason, a.Status, a.Notes, a.CreatedAt,
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       d.FullName AS DoctorName
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
ORDER BY a.AppointmentDateTime DESC;";

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
       CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
       d.FullName AS DoctorName
FROM Appointments a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE CAST(a.AppointmentDateTime AS date) = CAST(GETDATE() AS date)
ORDER BY a.AppointmentDateTime ASC;";

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

        private static Appointment MapAppointment(SqlDataReader reader)
        {
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
                PatientName = GetNullableString(reader, "PatientName"),
                DoctorName = GetNullableString(reader, "DoctorName")
            };
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                const string query = @"SELECT COUNT(1) FROM Appointments;";
                await using SqlConnection connection = CreateConnection();
                await connection.OpenAsync();

                await using SqlCommand command = new SqlCommand(query, connection);
                object? result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error fetching Appointment count: {ex.Message}");
                return 0;
            }
        }
    }
}