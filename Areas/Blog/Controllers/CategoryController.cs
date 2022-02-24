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
using AppMVC.ExtendMethods;

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

        [HttpGet("/admin/categories/{catId}/edit")]
        public async Task<IActionResult> Edit(int? catId)
        {
            if (catId == null) return NotFound("CategoryId not found");

            Category category = await _context.Categories.FindAsync(catId);
            if (category == null) return NotFound("Category not found");

            ViewBag.categories = new SelectList(await GetTreeCategory(), "Id", "Title", category.ParentId);
            return View(category);
        }
        [HttpPost("/admin/categories/{catId}/edit")]
        public async Task<IActionResult> Edit(int? catId, Category category)
        {
            if (catId != category.Id) return NotFound("CategoryId not found");

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.ParentId == -1) category.ParentId = null;

                    var qr = (from c in _context.Categories select c)
                                .Include(c => c.ChildrenCategory);

                    var fullCat = (await qr.ToListAsync())
                                    .Where(c => c.Id == catId)
                                    .FirstOrDefault();

                    bool canUpdate = true;
                    if (fullCat.ChildrenCategory?.Count > 0 && category.ParentId != null)
                    {
                        canUpdate = CheckIsChildrenCat(fullCat.ChildrenCategory.ToList(), category.ParentId.Value, canUpdate);
                    }

                    if (canUpdate && fullCat.Id != category.ParentId)
                    {
                        fullCat.ParentId = category.ParentId;
                        fullCat.Title = category.Title;
                        fullCat.Description = category.Description;
                        fullCat.Slug = category.Slug;

                        _context.Categories.Update(fullCat);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("Parent category is a children category");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) return NotFound("Category not found");
                    throw;
                }
            }

            ViewBag.categories = new SelectList(await GetTreeCategory(), "Id", "Title", category.ParentId);
            return View(category);
        }

        [HttpGet("/admin/categories/{catId}/delete")]
        public async Task<IActionResult> Delete(int? catId)
        {
            if (catId == null) return NotFound("CategoryId not found");

            Category category = await _context.Categories.FindAsync(catId);
            if (category == null) return NotFound("Category not found");

            return View(category);
        }
        [HttpPost("/admin/categories/{catId}/delete"), ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int? catId)
        {
            if (catId == null) return NotFound("CategoryId not found");

            Category category = await _context.Categories
                                    .Include(c => c.ChildrenCategory)
                                    .Include(c => c.ParentCategory)
                                    .FirstOrDefaultAsync(c => c.Id == catId);

            if (category == null) return NotFound("Category not found");

            try
            {
                if (category.ChildrenCategory?.Count > 0)
                {
                    foreach (var chilCat in category.ChildrenCategory)
                    {
                        chilCat.ParentId = category.ParentId;
                    }
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id)) return NotFound("Category not found");
                return View(category);
            }
        }

        [HttpGet("/admin/categories/{catId}")]
        public async Task<IActionResult> Details(int? catId)
        {
            if (catId == null) return NotFound("CategoryId not found");

            Category category = await _context.Categories
                                    .Include(c => c.ParentCategory)
                                    .Where(c => c.Id == catId)
                                    .FirstOrDefaultAsync();

            if (category == null) return NotFound("Category not found");

            return View(category);
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

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(c => c.Id == id);
        }

        private bool CheckIsChildrenCat(List<Category> childrenCats, int parentId, bool canUpdate)
        {
            foreach (var cat in childrenCats)
            {
                if (cat.Id == parentId)
                {
                    canUpdate = false;
                    break;
                }
                else
                {
                    if (cat.ChildrenCategory?.Count > 0)
                    {
                        canUpdate = CheckIsChildrenCat(cat.ChildrenCategory.ToList(), parentId, canUpdate);
                    }
                }
            }
            return canUpdate;
        }
    }
}
