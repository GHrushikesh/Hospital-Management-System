using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class HospitalBed
    {
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
        [StringLength(50)]
        [Display(Name = "Bed Type")]
        public string BedType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Floor / Wing")]
        public string Floor { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Vacant";

        public int? PatientId { get; set; }
        public int? AdmittedDoctorId { get; set; }

        [StringLength(300)]
        [Display(Name = "Clinical Admission Notes")]
        public string? AdmissionNotes { get; set; }

        public DateTime? AdmittedAt { get; set; }

        // Joined navigation/display properties
        public string? PatientName { get; set; }
        public string? PatientMRN { get; set; }
        public string? PatientPhoneNumber { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSpecialty { get; set; }
    }
}
