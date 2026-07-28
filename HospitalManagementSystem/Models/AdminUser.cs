using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class AdminUser
    {
        public int AdminUserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}