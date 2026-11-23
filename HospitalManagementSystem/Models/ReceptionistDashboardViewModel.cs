namespace HospitalManagementSystem.Models
{
    public class ReceptionistDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string CounterOrDesk { get; set; } = string.Empty;
        public string ShiftTiming { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;

        // Front desk summary counts for today
        public int TodayTokensIssued { get; set; }
        public int TodayWaitingInClinic { get; set; }
        public int TodayCompletedVisits { get; set; }
        public decimal TodayFeesCollected { get; set; }

        // Live Doctor OPD Roster
        public IReadOnlyList<DoctorQueueStatusViewModel> ActiveDoctors { get; set; } = Array.Empty<DoctorQueueStatusViewModel>();

        // Today's central appointments queue
        public IReadOnlyList<Appointment> TodayAppointments { get; set; } = Array.Empty<Appointment>();

        // Recent patient master index for search & verification
        public IReadOnlyList<Patient> RecentPatients { get; set; } = Array.Empty<Patient>();
    }

    public class DoctorQueueStatusViewModel
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public string Availability { get; set; } = string.Empty;
        public int TotalBookedToday { get; set; }
        public int PatientsWaiting { get; set; }
        public int NextTokenNumber { get; set; }
    }

    public class WalkInBookingViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }

        public int DoctorId { get; set; }
        public string Reason { get; set; } = "General Outpatient Consultation";

        // Payment details collected at the desk
        public decimal ConsultationFee { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public bool IsEmergency { get; set; } = false;
        public string? EmergencyDescription { get; set; }
    }
}
