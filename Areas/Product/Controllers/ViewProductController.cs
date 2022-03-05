using AppMVC.Models;
using AppMVC.Models.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppMVC.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ViewProductController> _logger;

        public ViewProductController(AppDbContext context, UserManager<AppUser> userManager, ILogger<ViewProductController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet("/products/{catSlug?}")]
        public async Task<IActionResult> Index(string catSlug, [FromQuery(Name = "p")] int page = 1)
        {
            ViewBag.catSlug = catSlug;
            ViewBag.categories = GetCategories();

            var products = _context.Products
                                .Include(p => p.Author)
                                .Include(p => p.Photos)
                                .Include(p => p.ProductCategories)
                                .ThenInclude(pc => pc.Category)
                                .OrderByDescending(p => p.DateUpdated)
                                .AsEnumerable();

            if (catSlug != null)
            {
                PCategory category = await _context.PCategories.FirstOrDefaultAsync(c => c.Slug == catSlug);
                if (category == null) return NotFound("Category not found");
                ViewBag.category = category;
                ViewBag.parentsList = category.GetParentList();

                var childrenCatIds = category.GetChildrenList().Select(c => c.Id);
                products = products.Where(p => p.ProductCategories.Any(pc => pc.Category.Id == category.Id || childrenCatIds.Contains(pc.Category.Id)));
            }

            // Pagination
            int PRODUCT_PER_PAGE = 9;
            int PAGE_RANGE = 5;
            int totalProductsNumber = products.Count();
            int totalPages = (int)Math.Ceiling((decimal)totalProductsNumber / PRODUCT_PER_PAGE);

            if (page <= 0) return RedirectToAction(nameof(Index), new { p = 1 });
            if (page > totalPages) return RedirectToAction(nameof(Index), new { p = totalPages });

            var paginationModel = new PaginationModel()
            {
                TotalPages = totalPages,
                CurrentPage = page,
                PageRange = PAGE_RANGE,
                GenerateUrl = (page) => Url.Action("Index", new { p = page })
            };

            ViewBag.paginationModel = paginationModel;
            products = products.Skip((page - 1) * PRODUCT_PER_PAGE).Take(PRODUCT_PER_PAGE);

            return View(products.ToList());
        }

        [HttpGet("/products/{proSlug}.html")]
        public async Task<IActionResult> Details(string proSlug)
        {
            ViewBag.categories = GetCategories();

            if (proSlug == null) return NotFound("Product slug not found");

            var product = await _context.Products
                                   .Where(p => p.Slug == proSlug)
                                   .Include(p => p.Author)
                                   .Include(p => p.Photos)
                                   .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                                   .FirstOrDefaultAsync();
            if (product == null) return NotFound("Product not found");

            PCategory category = product.ProductCategories.FirstOrDefault().Category;
            ViewBag.parentsList = category.GetParentList();
            ViewBag.catSlug = category.Slug;

            ViewBag.otherProducts = _context.Products
                                        .Include(p => p.ProductCategories)
                                        .Where(p => p.ProductCategories.FirstOrDefault().CategoryId == category.Id)
                                        .Where(p => p.Id != product.Id)
                                        .OrderByDescending(p => p.DateUpdated)
                                        .Take(5);

            var srcImgs = new List<string>();
            if (product.Photos.Any())
            {
                foreach (var photo in product.Photos)
                {
                    srcImgs.Add($"/uploads/Products/{photo.FileName}");
                }
            } else
            {
                srcImgs.Add("/uploads/no-photo.jpg");
            }
            ViewBag.srcImgs = srcImgs;

            return View(product);
        }
            private List<PCategory> GetCategories()
        {
            return _context.PCategories
                        .Include(c => c.ChildrenCategories)
                        .AsEnumerable()
                        .Where(c => c.ParentCategory == null)
                        .ToList();
        }
    }
}
