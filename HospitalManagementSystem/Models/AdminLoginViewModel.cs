using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class AdminLoginViewModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(256)]
        public string Password { get; set; } = string.Empty;
    }
}