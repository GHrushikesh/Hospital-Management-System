using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class StaffUser
    {
        public int StaffUserId { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Role { get; set; } = "Staff";

        [Required, StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [StringLength(30)]
        public string? EmployeeId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}
