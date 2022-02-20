using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMVC.Models;
using AppMVC.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using AppMVC.Areas.Identity.Data;
using System.Collections;
using Microsoft.Extensions.Logging;

namespace AppMVC.Areas.Blog.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    [Area("Blog")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet("/admin/categories")]
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.Categories select c)
                                .Include(c => c.ParentCategory)
                                .Include(c => c.ChildrenCategory);

            var categories = (await qr.ToListAsync())
                                .Where(c => c.ParentCategory == null)
                                .ToList();

            return View(categories);
        }

        [HttpGet("/admin/categories/create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = new SelectList(await GetTreeCategory(), "Id", "Title", -1);
            return View();
        }

        [HttpPost("/admin/categories/create")]
        public async Task<IActionResult> Create(Category category)
        {
            ViewBag.categories = new SelectList(await GetTreeCategory(), "Id", "Title", -1);

            if (ModelState.IsValid)
            {
                if (category.ParentId == -1) category.ParentId = null;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"{category.Title} category created.");

                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        private async Task<IEnumerable<Category>> GetTreeCategory(bool isIndex = false)
        {
            var qr = (from c in _context.Categories select c)
                                .Include(c => c.ParentCategory)
                                .Include(c => c.ChildrenCategory);

            var categories = (await qr.ToListAsync())
                                .Where(c => c.ParentCategory == null)
                                .ToList();

            List<Category> newCategories = new List<Category>();

            if (!isIndex) newCategories.Add(new Category() { Id = -1, Title = "None" });

            AddChildrenCategory(categories, newCategories, 0);
            return newCategories;
        }
        private void AddChildrenCategory(List<Category> categories, List<Category> newCategories, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("--", level));
            foreach (var category in categories)
            {
                Category tempCategory = new Category()
                {
                    Id = category.Id,
                    Title = $"{prefix} {category.Title}"
                };
                newCategories.Add(tempCategory);
                if (category.ChildrenCategory?.Count > 0)
                {
                    AddChildrenCategory(category.ChildrenCategory.ToList(), newCategories, level + 1);
                }
            }
        }
    }
}
