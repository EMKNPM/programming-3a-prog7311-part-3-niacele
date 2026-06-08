using System.ComponentModel.DataAnnotations;

namespace Global_Logistics_Management_System___ST10439898.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Staff Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
