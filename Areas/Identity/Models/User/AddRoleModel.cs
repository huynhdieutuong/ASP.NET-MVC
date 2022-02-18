using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppMVC.Models;

namespace AppMVC.Areas.Identity.Models.User
{
    public class AddRoleModel
    {
        public AppUser User { get; set; }

        [Display(Name = "Role names")]
        public string[] RoleNames { get; set; }
    }
}