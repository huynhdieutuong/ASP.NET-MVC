using AppMVC.Areas.Identity.Data;
using AppMVC.Areas.Product.Models;
using AppMVC.ExtendMethods;
using AppMVC.Models;
using AppMVC.Models.Product;
using AppMVC.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppMVC.Areas.Product.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    [Area("Product")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ProductController> _logger;

        public ProductController(AppDbContext context, UserManager<AppUser> userManager, ILogger<ProductController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet("/admin/products")]
        public async Task<ActionResult> Index([FromQuery(Name ="p")] int page = 1)
        {
            var qr = _context.Products
                        .Include(p => p.Author)
                        .Include(p => p.ProductCategories)
                        .ThenInclude(pc => pc.Category)
                        .OrderByDescending(p => p.DateUpdated);

            int PRODUCT_PER_PAGE = 5;
            int PAGE_RANGE = 5;
            int totalProductsNumber = qr.Count();
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

            var products = await qr.Skip((page - 1) * PRODUCT_PER_PAGE).Take(PRODUCT_PER_PAGE).ToListAsync();

            return View(products);
        }

        [HttpGet("/admin/products/{productId}")]
        public async Task<ActionResult> Details(int? productId)
        {
            if (productId == null) return NotFound("ProductId not found");

            var product = await _context.Products
                            .Include(p => p.Author)
                            .Include(p => p.ProductCategories)
                            .ThenInclude(pc => pc.Category)
                            .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound("Product not found");
            return View(product);
        }

        [HttpGet("/admin/products/create")]
        public async Task<ActionResult> Create()
        {
            ViewBag.categories = new MultiSelectList(await GetTreeCategories(), "Id", "Title");
            return View();
        }

        [HttpPost("/admin/products/create")]
        public async Task<ActionResult> Create(CreateProductModel product)
        {
            ViewBag.categories = new MultiSelectList(await GetTreeCategories(), "Id", "Title");

            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (_context.Products.Any(p => p.Slug == product.Slug))
            {
                ModelState.AddModelError("Duplicate slug");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                product.AuthorId = user.Id;
                product.DateCreated = product.DateUpdated = DateTime.Now;

                List<ProductCategory> productCategories = new List<ProductCategory>();
                if (product.CategoryIds.Count() > 0)
                {
                    foreach (var categoryId in product.CategoryIds)
                    {
                        productCategories.Add(new ProductCategory()
                        {
                            Product = product,
                            CategoryId = categoryId
                        });
                    }
                }

                try
                {
                    _context.Products.Add(product);
                    _context.ProductCategories.AddRange(productCategories);

                    await _context.SaveChangesAsync();

                    string message = $"{product.Title} product created.";
                    _logger.LogInformation(message);
                    StatusMessage = message;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return View(product);
                }
            }

            return View(product);
        }

        [HttpGet("/admin/products/{productId}/edit")]
        public async Task<ActionResult> Edit(int? productId)
        {
            ViewBag.categories = new MultiSelectList(await GetTreeCategories(), "Id", "Title");

            if (productId == null) return NotFound("ProductId not found");

            var product = await _context.Products
                            .Include(p => p.ProductCategories)
                            .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound("Product not found");

            var productEdit = new CreateProductModel()
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToArray(),
                Slug = product.Slug,
                Content = product.Content,
                Published = product.Published
            };

            return View(productEdit);
        }

        [HttpPost("/admin/products/{productId}/edit")]
        public async Task<ActionResult> Edit(int? productId, CreateProductModel product)
        {
            ViewBag.categories = new MultiSelectList(await GetTreeCategories(), "Id", "Title");

            if (productId == null) return NotFound("ProductId not found");
            if (productId != product.Id) return NotFound("Product not found");

            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (_context.Products.Any(p => p.Slug == product.Slug && p.Id != productId))
            {
                ModelState.AddModelError("Duplicate slug");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productEdit = await _context.Products
                                        .Include(p => p.ProductCategories)
                                        .FirstOrDefaultAsync(p => p.Id == productId);
                    if (productEdit == null) return NotFound("Product not found");

                    //Update product
                    productEdit.Title = product.Title;
                    productEdit.Content = product.Content;
                    productEdit.Description = product.Description;
                    productEdit.Price = product.Price;
                    productEdit.Published = product.Published;
                    productEdit.Slug = product.Slug;
                    productEdit.DateUpdated = DateTime.Now;

                    _context.Products.Update(productEdit);

                    //Update product category
                    if (product.CategoryIds == null) product.CategoryIds = new int[] { };

                    var oldProductCatIds = productEdit.ProductCategories.Select(pc => pc.CategoryId).ToArray();
                    var newProductCatIds = product.CategoryIds;

                    var removeProductCats = productEdit.ProductCategories.Where(pc => !newProductCatIds.Contains(pc.CategoryId));
                    _context.ProductCategories.RemoveRange(removeProductCats);

                    var addCatIds = newProductCatIds.Where(id => !oldProductCatIds.Contains(id));
                    foreach (var catId in addCatIds)
                    {
                        _context.ProductCategories.Add(new ProductCategory()
                        {
                            ProductId = product.Id,
                            CategoryId = catId
                        });
                    }

                    await _context.SaveChangesAsync();

                    string message = $"{product.Title} product updated.";
                    _logger.LogInformation(message);
                    StatusMessage = message;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return View(product);
                }
            }

            return View(product);
        }

        [HttpGet("/admin/products/{productId}/delete")]
        public async Task<ActionResult> Delete (int? productId)
        {
            if (productId == null) return NotFound("PostId not found");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound("Product not found");

            return View(product);
        }

        [HttpPost("/admin/products/{productId}/delete"), ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirm(int? productId)
        {
            if (productId == null) return NotFound("PostId not found");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound("Product not found");

            try
            {
                _context.Products.Remove(product);

                var productCategories = _context.ProductCategories.Where(pc => pc.ProductId == product.Id);
                _context.ProductCategories.RemoveRange(productCategories);

                await _context.SaveChangesAsync();

                StatusMessage = $"Deleted {product.Title} product";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View(product);
            }
        }

        private async Task<IEnumerable<PCategory>> GetTreeCategories()
        {
            List<PCategory> treeCategories = new List<PCategory>();
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
