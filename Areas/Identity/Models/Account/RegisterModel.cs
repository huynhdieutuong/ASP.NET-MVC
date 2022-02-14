using System.ComponentModel.DataAnnotations;

namespace AppMVC.Areas.Identity.Models.Account
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} must be between {2} - {1} chars", MinimumLength = 5)]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "{0} wrong")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "User name")]
        [DataType(DataType.Text)]
        [StringLength(100, ErrorMessage = "{0} must be between {2} - {1} chars", MinimumLength = 3)]
        public string UserName { get; set; }
    }
}