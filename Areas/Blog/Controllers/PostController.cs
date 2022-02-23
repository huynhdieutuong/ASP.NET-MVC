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
using Microsoft.AspNetCore.Identity;
using AppMVC.Areas.Blog.Models;
using AppMVC.Utilities;

namespace AppMVC.Areas.Blog.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    [Area("Blog")]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<CategoryController> _logger;

        public PostController(AppDbContext context, UserManager<AppUser> userManager, ILogger<CategoryController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet("/admin/posts")]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int page = 1)
        {
            var qr = _context.Posts
                            .Include(p => p.Author)
                            .Include(p => p.PostCategories)
                            .ThenInclude(pc => pc.Category)
                            .OrderByDescending(p => p.DateCreated);

            int POST_PER_PAGE = 5;
            int PAGE_RANGE = 5;
            int totalPostsNumber = qr.Count();
            int totalPages = (int)Math.Ceiling((decimal)totalPostsNumber / POST_PER_PAGE);

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

            var posts = await qr.Skip((page - 1) * POST_PER_PAGE).Take(POST_PER_PAGE).ToListAsync();

            return View(posts);
        }

        [HttpGet("/admin/posts/{postId}")]
        public async Task<IActionResult> Details(int? postId)
        {
            if (postId == null) return NotFound("PostId not found");

            var post = await _context.Posts
                            .Include(p => p.Author)
                            .Include(p => p.PostCategories)
                            .ThenInclude(pc => pc.Category)
                            .FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return NotFound("Post not found");

            return View(post);
        }

        [HttpGet("/admin/posts/create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = new MultiSelectList(await GetTreeCategory(), "Id", "Title");
            return View();
        }
        [HttpPost("/admin/posts/create")]
        public async Task<IActionResult> Create(CreatePostModel post)
        {
            ViewBag.categories = new MultiSelectList(await GetTreeCategory(), "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Duplicated url. Please enter another url");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;
                _context.Posts.Add(post);

                if (post.CategoryIds != null)
                {
                    foreach (var catId in post.CategoryIds)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            Post = post,
                            CategoryId = catId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = $"Created {post.Title}";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        private async Task<IEnumerable<Category>> GetTreeCategory()
        {
            var qr = _context.Categories
                        .Include(c => c.ParentCategory)
                        .Include(c => c.ChildrenCategory);

            var categories = (await qr.ToListAsync()).Where(c => c.ParentId == null).ToList();

            List<Category> newCategories = new List<Category>();

            AddChildrenCategory(categories, newCategories, 0);

            return newCategories;
        }
        private void AddChildrenCategory(List<Category> categories, List<Category> newCategories, int level)
        {
            var prefix = string.Concat(Enumerable.Repeat("--", level));
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
