using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your username")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }
}
