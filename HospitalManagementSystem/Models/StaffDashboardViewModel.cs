namespace HospitalManagementSystem.Models
{
    public class StaffDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Nurse";
        public string Department { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string AssignedRoomOrWard { get; set; } = string.Empty;
        public string ShiftTiming { get; set; } = string.Empty;
        public string? HiredByDoctorName { get; set; }

        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int VacantBeds { get; set; }

        public IReadOnlyList<Appointment> TodayAppointments { get; set; } = Array.Empty<Appointment>();
        public IReadOnlyList<Patient> RecentPatients { get; set; } = Array.Empty<Patient>();
        public IReadOnlyList<HospitalBed> OccupiedBedsList { get; set; } = Array.Empty<HospitalBed>();
        public IReadOnlyList<Prescription> ActivePrescriptionOrders { get; set; } = Array.Empty<Prescription>();
        public IReadOnlyList<string> Tasks { get; set; } = Array.Empty<string>();
    }
}
