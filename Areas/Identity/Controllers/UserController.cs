using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMVC.Areas.Identity.Data;
using AppMVC.Areas.Identity.Models.User;
using AppMVC.ExtendMethods;
using AppMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppMVC.Areas.Identity.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    [Area("Identity")]
    public class UserController : Controller
    {
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet("/users")]
        public async Task<IActionResult> Index()
        {
            var model = new UserListModel();
            model.Users = await _userManager.Users
                                .Select(u => new UserAndRole()
                                {
                                    Id = u.Id,
                                    UserName = u.UserName
                                })
                                .ToListAsync();

            foreach (var user in model.Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.RoleNames = string.Join(",", roles);
            }

            return View(model);
        }

        [HttpGet("/users/{userId}/add-role")]
        public async Task<IActionResult> AddRole(string userId)
        {
            var model = new AddRoleModel();
            if (string.IsNullOrEmpty(userId)) return NotFound("UserId not found");

            model.User = await _userManager.FindByIdAsync(userId);
            if (model.User == null) return NotFound("User not found");

            model.RoleNames = (await _userManager.GetRolesAsync(model.User)).ToArray<string>();

            // Get all roles
            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.allRoles = new SelectList(roleNames);

            return View(model);
        }
        [HttpPost("/users/{userId}/add-role"), ActionName("AddRole")]
        public async Task<IActionResult> AddRoleConfirm(string userId, AddRoleModel model)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound("UserId not found");

            model.User = await _userManager.FindByIdAsync(userId);
            if (model.User == null) return NotFound("User not found");

            var oldRoles = (await _userManager.GetRolesAsync(model.User)).ToArray<string>();

            // Get all roles
            List<string> allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.allRoles = new SelectList(allRoles);

            var removeRoles = oldRoles.Where(r => !model.RoleNames.Contains(r));
            var removeResult = await _userManager.RemoveFromRolesAsync(model.User, removeRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError(removeResult);
                return View(model);
            }

            var addRoles = model.RoleNames.Where(r => !oldRoles.Contains(r));
            var addResult = await _userManager.AddToRolesAsync(model.User, addRoles);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(addResult);
                return View(model);
            }

            StatusMessage = $"Updated roles for user: {model.User.UserName}";
            return RedirectToAction(nameof(Index));
        }
    }
}