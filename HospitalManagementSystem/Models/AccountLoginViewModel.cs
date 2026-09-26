using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class AccountLoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
