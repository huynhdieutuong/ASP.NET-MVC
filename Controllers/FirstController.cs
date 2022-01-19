using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppMVC.Controllers
{
    public class FirstController : Controller
    {
        private readonly ILogger<FirstController> _logger;

        public FirstController(ILogger<FirstController> logger)
        {
            _logger = logger;
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
        // ViewResult                  | View()

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
    }
}