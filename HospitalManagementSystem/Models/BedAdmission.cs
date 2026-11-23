using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class BedAdmission
    {
        public int AdmissionId { get; set; }

        [Required]
        public int BedId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Bed Number")]
        public string BedNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Ward / Unit")]
        public string WardName { get; set; } = string.Empty;

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int AdmittedDoctorId { get; set; }

        [StringLength(500)]
        [Display(Name = "Admission Diagnosis / Notes")]
        public string? AdmissionNotes { get; set; }

        public DateTime AdmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DischargedAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Physician Discharge Summary")]
        public string? DischargeSummary { get; set; }

        [StringLength(150)]
        [Display(Name = "Discharging Staff / Nurse")]
        public string? DischargedByStaff { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Active"; // Active, Discharged

        // Navigation / Joined Properties
        public string? PatientName { get; set; }
        public string? PatientMRN { get; set; }
        public string? PatientPhoneNumber { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSpecialty { get; set; }

        public string LengthOfStay
        {
            get
            {
                var end = DischargedAt ?? DateTime.UtcNow;
                var span = end - AdmittedAt;
                if (span.TotalDays >= 1)
                {
                    return $"{(int)span.TotalDays}d {span.Hours}h";
                }
                return $"{Math.Max(1, (int)span.TotalHours)}h {span.Minutes}m";
            }
        }
    }
}
