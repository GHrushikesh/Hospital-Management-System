using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class StaffUser
    {
        public int StaffUserId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Role { get; set; } = "Staff";

        [Required, StringLength(30)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [StringLength(150)]
        [Display(Name = "Qualification & Certifications")]
        public string Qualification { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Assigned Room / Ward")]
        public string AssignedRoomOrWard { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Designated Shift Timing")]
        public string ShiftTiming { get; set; } = string.Empty;

        public int? HiredByDoctorId { get; set; }

        [StringLength(150)]
        [Display(Name = "Hiring Doctor / Clinical Supervisor")]
        public string? HiredByDoctorName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}
