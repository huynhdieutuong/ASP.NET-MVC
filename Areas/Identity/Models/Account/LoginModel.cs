using System.ComponentModel.DataAnnotations;

namespace AppMVC.Areas.Identity.Models.Account
{
    public class LoginModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "User name or email")]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}