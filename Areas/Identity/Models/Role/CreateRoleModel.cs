using System.ComponentModel.DataAnnotations;

namespace AppMVC.Areas.Identity.Models.Role
{
    public class CreateRoleModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Role name")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} must be between {2} - {1} chars")]
        public string Name { get; set; }
    }
}