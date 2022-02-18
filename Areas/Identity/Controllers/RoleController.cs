using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMVC.Areas.Identity.Models.Role;
using AppMVC.ExtendMethods;
using AppMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppMVC.Areas.Identity.Controllers
{
    [Authorize]
    [Area("Identity")]
    public class RoleController : Controller
    {
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RoleController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet("/roles")]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            var roleList = new List<RoleModel>();
            foreach (var role in roles)
            {
                var rm = new RoleModel()
                {
                    Name = role.Name,
                    Id = role.Id
                };
                roleList.Add(rm);
            }
            return View(roleList);
        }

        [HttpGet("/roles/create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost("/roles/create")]
        public async Task<IActionResult> Create(CreateRoleModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole(model.Name);
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    StatusMessage = $"{model.Name} role created";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(result);
                }
            }
            return View();
        }
        [HttpGet("/roles/{roleId}/edit")]
        public async Task<IActionResult> Edit(string roleId, EditRoleModel model)
        {
            if (roleId == null) return NotFound("RoleId not found");
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound("Role not found");
            model.Name = role.Name;
            // model.role = role;
            ModelState.Clear();
            return View(model);
        }
        [HttpPost("/roles/{roleId}/edit"), ActionName("Edit")]
        public async Task<IActionResult> EditConfirm(string roleId, EditRoleModel model)
        {
            if (roleId == null) return NotFound("RoleId not found");
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound("Role not found");

            if (!ModelState.IsValid) return View(model);

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                StatusMessage = $"{role.Name} updated";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(result);
            return View(model);
        }

        [HttpGet("/roles/{roleId}/delete")]
        public async Task<IActionResult> Delete(string roleId)
        {
            if (roleId == null) return NotFound("RoleId not found");
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound("Role not found");

            return View(role);
        }
        [HttpPost("/roles/{roleId}/delete"), ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(string roleId)
        {
            if (roleId == null) return NotFound("RoleId not found");
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound("Role not found");

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                StatusMessage = $"{role.Name} deleted";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(result);
            return View();
        }
    }
}