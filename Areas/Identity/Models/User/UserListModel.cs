using System.Collections.Generic;
using AppMVC.Models;

namespace AppMVC.Areas.Identity.Models.User
{
    public class UserListModel
    {
        public List<UserAndRole> Users { get; set; }
    }

    public class UserAndRole : AppUser
    {
        public string RoleNames { get; set; }
    }
}