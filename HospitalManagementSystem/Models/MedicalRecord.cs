using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class MedicalRecord
    {
        public int MedicalRecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public int? AppointmentId { get; set; }

        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Treatment { get; set; }

        [StringLength(1000)]
        public string? Prescription { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime RecordDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? PatientName { get; set; }

        public string? DoctorName { get; set; }
    }
}