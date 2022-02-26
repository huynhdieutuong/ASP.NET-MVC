using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppMVC.Models;
using Microsoft.Extensions.Logging;
using AppMVC.Models.Blog;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public ViewPostController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Route("/posts/{catSlug?}")]
        public IActionResult Index(string catSlug, [FromQuery(Name = "p")] int page = 1)
        {
            ViewBag.categories = GetCategories();
            ViewBag.catSlug = catSlug;

            Category category = null;
            if (!string.IsNullOrEmpty(catSlug))
            {
                category = _context.Categories
                                .Where(c => c.Slug == catSlug)
                                .Include(c => c.ChildrenCategory)
                                .FirstOrDefault();

                if (category == null) return NotFound("Category not found");
            }

            ViewBag.category = category;

            var posts = _context.Posts
                            .Include(p => p.Author)
                            .Include(p => p.PostCategories)
                            .ThenInclude(pc => pc.Category)
                            .OrderByDescending(p => p.DateUpdated)
                            .AsQueryable();

            if (category != null)
            {
                List<int> catIds = new List<int>();
                catIds.Add(category.Id);
                category.GetChildIds(category.ChildrenCategory, catIds);

                posts = posts.Where(p => p.PostCategories.Any(pc => catIds.Contains(pc.CategoryId)));
            }

            // Pagination
            int POST_PER_PAGE = 5;
            int PAGE_RANGE = 5;
            int totalPostsNumber = posts.Count();
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
            posts = posts.Skip((page - 1) * POST_PER_PAGE).Take(POST_PER_PAGE);

            ViewBag.parentsList = category?.GetParentsList();

            return View(posts.ToList());
        }

        [Route("/posts/{postSlug}.html")]
        public IActionResult Details(string postSlug)
        {
            if (postSlug == null) return NotFound("PostSlug not found");

            var post = _context.Posts
                        .Where(p => p.Slug == postSlug)
                        .Include(p => p.Author)
                        .Include(p => p.PostCategories)
                        .ThenInclude(pc => pc.Category)
                        .FirstOrDefault();
            if (post == null) return NotFound("Post not found");

            // Get other posts the same category
            List<int> postCatIds = post.PostCategories.Select(pc => pc.CategoryId).ToList();
            List<Post> otherPosts = _context.Posts
                                        .Where(p => p.PostCategories.Any(pc => postCatIds.Contains(pc.CategoryId)))
                                        .Where(p => p.Id != post.Id)
                                        .OrderByDescending(p => p.DateUpdated)
                                        .Take(5)
                                        .ToList();

            ViewBag.categories = GetCategories();
            ViewBag.parentsList = post.PostCategories.FirstOrDefault()?.Category.GetParentsList();
            ViewBag.otherPosts = otherPosts;

            return View(post);
        }

        private List<Category> GetCategories()
        {
            return _context.Categories
                        .Include(c => c.ChildrenCategory)
                        .AsEnumerable()
                        .Where(c => c.ParentCategory == null)
                        .ToList();
        }
    }
}