using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class EmergencyRequestViewModel
    {
        [Required(ErrorMessage = "Patient First Name is required.")]
        [StringLength(100)]
        [Display(Name = "Patient First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Patient Last Name is required.")]
        [StringLength(100)]
        [Display(Name = "Patient Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(20)]
        public string Gender { get; set; } = "Male";

        [Required(ErrorMessage = "Contact phone number is required.")]
        [StringLength(20)]
        [Display(Name = "Emergency Contact Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the acute trauma or emergency category.")]
        [Display(Name = "Emergency Category / Trauma Type")]
        public string EmergencyCategory { get; set; } = "Cardiac Distress / Acute Chest Pain";

        [Required(ErrorMessage = "Please describe the patient's critical symptoms.")]
        [StringLength(600)]
        [Display(Name = "Emergency Symptoms & Patient Condition")]
        public string EmergencyDescription { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Consciousness Level")]
        public string ConsciousnessLevel { get; set; } = "Conscious & Alert";

        [Required]
        [Display(Name = "Breathing Status")]
        public string BreathingStatus { get; set; } = "Normal Breathing";

        [Required]
        [Display(Name = "Estimated Arrival Time (ETA)")]
        public string EstimatedArrival { get; set; } = "Already at Emergency Gate";

        [Required]
        [Display(Name = "Reporting Person")]
        public string CallerRelationship { get; set; } = "Patient";

        [StringLength(100)]
        [Display(Name = "Caller / Bystander Name (Optional)")]
        public string? CallerName { get; set; }

        [Display(Name = "Specific Specialist (Optional)")]
        public int? PreferredDoctorId { get; set; }
    }
}
