namespace HospitalManagementSystem.Models
{
    public class DashboardViewModel
    {
        public int PatientCount { get; set; }
        public int DoctorCount { get; set; }
        public int AppointmentCount { get; set; }
        public IReadOnlyList<Appointment> TodayAppointments { get; set; } = Array.Empty<Appointment>();
    }
}
