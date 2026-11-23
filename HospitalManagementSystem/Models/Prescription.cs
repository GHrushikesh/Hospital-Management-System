using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public int? AppointmentId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Medicine Name")]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Dosage")]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Frequency")]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Duration")]
        public string Duration { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Instructions / Precautions")]
        public string? Instructions { get; set; }

        public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;

        // Joined navigation/display properties
        public string? PatientName { get; set; }
        public string? PatientMRN { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSpecialty { get; set; }
    }
}
