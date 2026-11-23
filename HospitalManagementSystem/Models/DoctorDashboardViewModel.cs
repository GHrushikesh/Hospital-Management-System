namespace HospitalManagementSystem.Models
{
    public class DoctorDashboardViewModel
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public int HiredStaffCount { get; set; }

        public IReadOnlyList<Appointment> PendingRequests { get; set; } = Array.Empty<Appointment>();
        public IReadOnlyList<Appointment> TodayAppointments { get; set; } = Array.Empty<Appointment>();
        public IReadOnlyList<Appointment> EmergencyAlerts { get; set; } = Array.Empty<Appointment>();
        public IReadOnlyList<Appointment> ScheduleHistory { get; set; } = Array.Empty<Appointment>();
    }
}
