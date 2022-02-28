using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMVC.Areas.Identity.Data;
using AppMVC.ExtendMethods;
using AppMVC.Models;
using AppMVC.Models.Product;
using AppMVC.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppMVC.Areas.Product.Controllers
{
    [Area("Product")]
    [Authorize(Roles = RoleNames.Admin)]
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

        [HttpGet("/admin/product-categories")]
        public IActionResult Index()
        {
            var categories = _context.PCategories
                                .Include(c => c.ChildrenCategories)
                                .AsEnumerable()
                                .Where(c => c.ParentId == null);
            return View(categories);
        }

        [HttpGet("/admin/product-categories/create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = new SelectList(await GetTreeCategories(), "Id", "Title", -1);

            return View();
        }

        [HttpPost("/admin/product-categories/create")]
        public async Task<IActionResult> Create(PCategory category)
        {
            ViewBag.categories = new SelectList(await GetTreeCategories(), "Id", "Title", -1);

            if (ModelState.IsValid)
            {
                if (category.ParentId == -1) category.ParentId = null;

                if (category.Slug == null)
                {
                    category.Slug = AppUtilities.GenerateSlug(category.Title);
                }

                if (_context.PCategories.Any(c => c.Slug == category.Slug))
                {
                    ModelState.AddModelError("Duplicate slug");
                    return View();
                }

                try
                {
                    _context.PCategories.Add(category);
                    await _context.SaveChangesAsync();

                    string message = $"{category.Title} category created.";
                    _logger.LogInformation(message);
                    StatusMessage = message;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return View();
                }
            }

            return View();
        }

        [HttpGet("/admin/product-categories/{catId}")]
        public async Task<IActionResult> Details(int? catId)
        {
            if (catId == null) return NotFound("CatId not found");

            var category = await _context.PCategories
                                    .Include(c => c.ParentCategory)
                                    .FirstOrDefaultAsync(c => c.Id == catId);
            if (category == null) return NotFound("Category not found");

            return View(category);
        }

        [HttpGet("/admin/product-categories/{catId}/edit")]
        public async Task<IActionResult> Edit(int? catId)
        {
            if (catId == null) return NotFound("CatId not found");

            var category = await _context.PCategories.FindAsync(catId);
            if (category == null) return NotFound("Category not found");

            ViewBag.categories = new SelectList(await GetTreeCategories(), "Id", "Title", category.ParentId);

            return View(category);
        }

        [HttpPost("/admin/product-categories/{catId}/edit")]
        public async Task<IActionResult> Edit(int? catId, PCategory category)
        {
            ViewBag.categories = new SelectList(await GetTreeCategories(), "Id", "Title", category.ParentId);

            if (catId != category.Id) return NotFound("CatId not found");

            if (ModelState.IsValid)
            {
                if (category.ParentId == -1) category.ParentId = null;

                if (category.Slug == null)
                {
                    category.Slug = AppUtilities.GenerateSlug(category.Title);
                }

                if (_context.PCategories.Any(c => c.Slug == category.Slug && c.Id != catId))
                {
                    ModelState.AddModelError("Duplicate slug");
                    return View(category);
                }

                var currentCategory = await _context.PCategories.FindAsync(catId);

                var childrenList = currentCategory.GetChildrenList();
                if (childrenList.Any(c => c.Id == category.ParentId))
                {
                    ModelState.AddModelError("Can't add Children like a Parent");
                    return View(category);
                }

                if (category.Id == category.ParentId)
                {
                    ModelState.AddModelError("Can't add itself like a Parent");
                    return View(category);
                }

                currentCategory.Title = category.Title;
                currentCategory.ParentId = category.ParentId;
                currentCategory.Description = category.Description;
                currentCategory.Slug = category.Slug;

                try
                {
                    _context.Update(currentCategory);
                    await _context.SaveChangesAsync();

                    string message = $"{currentCategory.Title} category updated.";
                    _logger.LogInformation(message);
                    StatusMessage = message;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return View(category);
                }
            }

            return View(category);
        }

        [HttpGet("/admin/product-categories/{catId}/delete")]
        public async Task<IActionResult> Delete(int? catId)
        {
            if (catId == null) return NotFound("CatId not found");

            var category = await _context.PCategories.FindAsync(catId);
            if (category == null) return NotFound("Category not found");

            return View(category);
        }

        [HttpPost("/admin/product-categories/{catId}/delete"), ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int? catId)
        {
            if (catId == null) return NotFound("CatId not found");

            var category = await _context.PCategories
                                .Include(c => c.ChildrenCategories)
                                .FirstOrDefaultAsync(c => c.Id == catId);
            if (category == null) return NotFound("Category not found");

            try
            {
                if (category.ChildrenCategories?.Count > 0)
                {
                    foreach (var childCategory in category.ChildrenCategories)
                    {
                        childCategory.ParentId = category.ParentId;
                    }
                }
                _context.PCategories.Remove(category);

                await _context.SaveChangesAsync();

                string message = $"{category.Title} category deleted.";
                _logger.LogInformation(message);
                StatusMessage = message;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View(category);
            }
        }

        private async Task<IEnumerable<PCategory>> GetTreeCategories()
        {
            List<PCategory> treeCategories = new List<PCategory>();
            treeCategories.Add(new PCategory() { Id = -1, Title = "None" });
            var qr = _context.PCategories.Include(c => c.ChildrenCategories);
            var originalCategories = (await qr.ToListAsync()).Where(c => c.ParentId == null).ToList();

            AddChilCategory(treeCategories, originalCategories, 0);
            return treeCategories;
        }

        private void AddChilCategory(List<PCategory> treeCategories, IEnumerable<PCategory> categories, int level)
        {
            foreach (var category in categories)
            {
                var prefix = string.Concat(Enumerable.Repeat("--", level));
                treeCategories.Add(new PCategory()
                {
                    Id = category.Id,
                    Title = prefix + " " + category.Title,
                });
                if (category.ChildrenCategories?.Count > 0)
                {
                    AddChilCategory(treeCategories, category.ChildrenCategories, level + 1);
                }
            }
        }
    }
}