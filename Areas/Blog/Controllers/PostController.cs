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
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public PostController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
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
    }
}
