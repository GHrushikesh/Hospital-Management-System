using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Appointment date and time are required.")]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        [StringLength(300)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Requested";

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsEmergency { get; set; } = false;

        [StringLength(500)]
        public string? EmergencyDescription { get; set; }

        public bool EmergencyAcknowledged { get; set; } = false;

        [StringLength(1000)]
        public string? DoctorNotes { get; set; }

        [StringLength(300)]
        public string? VitalsSummary { get; set; }

        [StringLength(50)]
        public string? ESITriageLevel { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        // Front Desk & Reception Workflow Properties
        public int? TokenNumber { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        public decimal PaymentAmount { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        // Associated / Display Properties
        public string? PatientName { get; set; }
        public string? PatientPhoneNumber { get; set; }
        public string? PatientMRN { get; set; }

        public string? DoctorName { get; set; }
        public string? DoctorSpecialty { get; set; }
        public string? DoctorQualification { get; set; }
        public string? DoctorRoomNumber { get; set; }
        public decimal DoctorConsultationFee { get; set; }
    }
}
