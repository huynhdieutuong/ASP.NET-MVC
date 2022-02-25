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
        public IActionResult Index(string catSlug)
        {
            var categories = _context.Categories
                                .Include(c => c.ChildrenCategory)
                                .AsEnumerable()
                                .Where(c => c.ParentCategory == null)
                                .ToList();

            ViewBag.categories = categories;
            ViewBag.catSlug = catSlug;

            return View();
        }

        [Route("/posts/{postSlug}.html")]
        public IActionResult Details(string postSlug)
        {
            return View();
        }
    }
}