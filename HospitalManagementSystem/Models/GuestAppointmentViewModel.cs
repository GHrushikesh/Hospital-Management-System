using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class GuestAppointmentViewModel
    {
        [Required(ErrorMessage = "First name is required."), StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required."), StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required."), StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valid phone number is required."), StringLength(20)]
        [Display(Name = "Phone / Mobile Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress, StringLength(150)]
        [Display(Name = "Email Address (Optional)")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please select a medical doctor / specialist.")]
        [Display(Name = "Consulting Specialist")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Please choose a desired appointment date & time.")]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDateTime { get; set; } = DateTime.Now.AddHours(2);

        [Required(ErrorMessage = "Please mention reason for appointment."), StringLength(300)]
        [Display(Name = "Chief Complaint / Reason for Visit")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Additional Notes (Optional)")]
        public string? Notes { get; set; }

        [Display(Name = "Mark as Critical Medical Emergency")]
        public bool IsEmergency { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Emergency Symptoms & Critical Condition Description")]
        public string? EmergencyDescription { get; set; }
    }
}
