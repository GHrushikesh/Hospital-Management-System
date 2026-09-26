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

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50)]
        public string Role { get; set; } = "Staff";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}
