using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMVC.Areas.Identity.Data;
using AppMVC.Models;
using AppMVC.Models.Blog;
using AppMVC.Models.Product;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Areas.Database.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;

        public DbManageController(AppDbContext dbContext, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success = await _dbContext.Database.EnsureDeletedAsync();

            StatusMessage = success ? "Delete database succeed" : "Delete database failed";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MigrateAsync()
        {
            await _dbContext.Database.MigrateAsync();

            StatusMessage = "Update database succeed";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SeedDataAsync()
        {
            //await SeedPostCategory();
            await SeedProductCategory();

            StatusMessage = "Seeded Database";

            return RedirectToAction(nameof(Index));
        }

        private async Task SeedProductCategory()
        {
            _dbContext.PCategories.RemoveRange(_dbContext.PCategories.Where(c => c.Description.Contains("[fakeData]")));
            _dbContext.Products.RemoveRange(_dbContext.Products.Where(c => c.Description.Contains("[fakeData]")));
            _dbContext.SaveChanges();

            //Fake product categories
            var fakerCategory = new Faker<PCategory>();
            int numCat = 1;
            fakerCategory.RuleFor(c => c.Title, f => $"Product category {numCat++} " + f.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, f => f.Lorem.Sentences(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, f => f.Lorem.Slug());

            var cat1 = fakerCategory.Generate();
            var cat11 = fakerCategory.Generate();
            var cat12 = fakerCategory.Generate();
            var cat111 = fakerCategory.Generate();
            var cat2 = fakerCategory.Generate();
            var cat21 = fakerCategory.Generate();
            var cat22 = fakerCategory.Generate();
            var cat221 = fakerCategory.Generate();

            cat11.ParentCategory = cat1;
            cat12.ParentCategory = cat1;
            cat111.ParentCategory = cat11;
            cat21.ParentCategory = cat2;
            cat22.ParentCategory = cat2;
            cat221.ParentCategory = cat22;

            var categories = new PCategory[] { cat1, cat11, cat12, cat111, cat2, cat21, cat22, cat221 };
            await _dbContext.PCategories.AddRangeAsync(categories);

            // Fake products
            var randomCatIndex = new Random();
            int numProduct = 1;
            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerProduct = new Faker<ProductModel>();
            fakerProduct.RuleFor(p => p.Title, f => $"Product {numProduct++} " + f.Commerce.ProductName());
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentences(5) + "fakeData");
            fakerProduct.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7));
            fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Price, f => int.Parse(f.Commerce.Price(500, 1000, 0)));
            fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2022, 2, 1)));

            List<ProductModel> products = new List<ProductModel>();
            List<ProductCategory> productCategories = new List<ProductCategory>();

            for (int i = 0; i < 40; i++)
            {
                var product = fakerProduct.Generate();
                product.DateUpdated = product.DateCreated;
                products.Add(product);

                productCategories.Add(new ProductCategory()
                {
                    Product = product,
                    Category = categories[randomCatIndex.Next(8)]
                });
            }
            await _dbContext.AddRangeAsync(products);
            await _dbContext.AddRangeAsync(productCategories);

            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedPostCategory()
        {
            // Remove fake categories and posts before fake
            _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Description.Contains("[fakeData]")));
            _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(p => p.Content.Contains("[fakeData]")));
            _dbContext.SaveChanges();

            // Fake categories
            var fakerCategory = new Faker<Category>();
            int numCat = 1;
            fakerCategory.RuleFor(c => c.Title, f => $"Category {numCat++} " + f.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, f => f.Lorem.Sentences(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, f => f.Lorem.Slug());

            var cat1 = fakerCategory.Generate();
            var cat11 = fakerCategory.Generate();
            var cat12 = fakerCategory.Generate();
            var cat2 = fakerCategory.Generate();
            var cat21 = fakerCategory.Generate();
            var cat211 = fakerCategory.Generate();

            cat11.ParentCategory = cat1;
            cat12.ParentCategory = cat1;
            cat21.ParentCategory = cat2;
            cat211.ParentCategory = cat21;

            var categories = new Category[] { cat1, cat11, cat12, cat2, cat21, cat211 };
            await _dbContext.Categories.AddRangeAsync(categories);

            // Fake posts
            var randomCatIndex = new Random();
            int numPost = 1;
            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2022, 2, 1)));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Post {numPost++} " + f.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);

                postCategories.Add(new PostCategory()
                {
                    Post = post,
                    Category = categories[randomCatIndex.Next(6)]
                });
            }
            await _dbContext.Posts.AddRangeAsync(posts);
            await _dbContext.PostCategories.AddRangeAsync(postCategories);

            await _dbContext.SaveChangesAsync();
        }


    }
}