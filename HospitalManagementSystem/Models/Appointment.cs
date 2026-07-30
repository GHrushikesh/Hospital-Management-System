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

        public DateTime AppointmentDateTime { get; set; }

        [Required]
        [StringLength(300)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Scheduled";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? PatientName { get; set; }

        public string? DoctorName { get; set; }
    }
}