using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AppMVC.Areas.Identity.Models.Role
{
    public class EditRoleModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Role name")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} must be between {2} - {1} chars")]
        public string Name { get; set; }

        // public IdentityRole role { get; set; }
    }
}