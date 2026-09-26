using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class GuestAppointmentViewModel
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required, StringLength(300)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
