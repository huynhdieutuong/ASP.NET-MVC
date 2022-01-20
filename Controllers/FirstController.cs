using System;
using System.IO;
using System.Linq;
using AppMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppMVC.Controllers
{
    public class FirstController : Controller
    {
        private readonly ILogger<FirstController> _logger;
        private readonly ProductService _productService;

        public FirstController(ILogger<FirstController> logger, ProductService productService)
        {
            _logger = logger;
            _productService = productService; // 3.4 Inject Service
        }

        public string Index()
        {
            /* 1.1 Controller have these properties:
                this.HttpContext
                this.Request
                this.Response
                this.RouteData

                this.User
                this.ModelState
                this.ViewData
                this.ViewBag
                this.Url
                this.TempData
            */

            _logger.LogInformation("Index action");
            _logger.LogWarning("Index action");
            _logger.LogError("Index action");
            _logger.LogCritical("Index action");
            _logger.LogDebug("Index action");

            return "This is first controller";
        }

        public void Nothing()
        {
            _logger.LogInformation("Nothing action");
            Response.Headers.Add("hi", "Hello everyone.");
        }

        public object Anything() => new int[] { 1, 2, 3 };

        // 1.2 IActionResult:
        // ContentResult               | Content()
        // EmptyResult                 | new EmptyResult()
        // FileResult                  | File()
        // ForbidResult                | Forbid()
        // JsonResult                  | Json()
        // LocalRedirectResult         | LocalRedirect()
        // RedirectResult              | Redirect()
        // RedirectToActionResult      | RedirectToAction()
        // RedirectToPageResult        | RedirectToRoute()
        // RedirectToRouteResult       | RedirectToPage()
        // PartialViewResult           | PartialView()
        // ViewComponentResult         | ViewComponent()
        // StatusCodeResult            | StatusCode()

        public IActionResult Readme()
        {
            var content = @"
                Hello,
                My name is Tuong.
                I am learning ASP.NET MVC
            ";
            return Content(content, "text/plain");
        }

        public IActionResult Bird()
        {
            string filePath = Path.Combine(Startup.ContentRootPath, "Files", "birds.png");
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "image/jpg");
        }

        public IActionResult IphonePrice()
        {
            return Json(
                new
                {
                    productName = "Iphone X",
                    price = 1000
                }
            );
        }

        public IActionResult Privacy()
        {
            var url = Url.Action("Privacy", "Home");
            _logger.LogInformation("Redirect to " + url);
            return LocalRedirect(url);
        }

        public IActionResult Google()
        {
            var url = "https://google.com";
            _logger.LogInformation("Redirect to " + url);
            return Redirect(url);
        }

        // 2.1 ViewResult                  | View()
        public IActionResult HelloView(string userName)
        {
            if (string.IsNullOrEmpty(userName)) userName = "Guest";

            // View(template) - direct url: /MyView/Hello1.cshtml
            // return View("/MyView/Hello1.cshtml", userName);

            // View(Hello2) -> /View/First/Hello2.cshtml
            // return View("Hello2", userName);

            // View() -> /View/First/HelloView.cshtml
            // return View((object)userName);

            // 2.2 View() -> /MyView/First/Hello3.cshtml
            return View("Hello3", userName);
        }

        public IActionResult ViewProduct(int? id)
        {
            // 3.5 use Service to find Product and transfer to View
            var product = _productService.Where(p => p.Id == id).FirstOrDefault();
            if (product == null) return NotFound("Product not found");

            // /View/First/ViewProduct.cshtml or
            // /MyView/First/ViewProduct.cshtml 
            return View(product);
        }
    }
}