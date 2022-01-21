using System.Linq;
using AppMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppMVC.Controllers
{
    [Route("solar-system/[action]")]
    public class PlanetController : Controller
    {
        private readonly ILogger<PlanetController> _logger;
        private readonly PlanetService _planetService;

        public PlanetController(ILogger<PlanetController> logger, PlanetService planetService)
        {
            _logger = logger;
            _planetService = planetService;
        }

        // [Route("planet-list")] // => /planet-list => /solar-system/planet-list
        [Route("/planet-list")] // => /planet-list
        public IActionResult Index()
        {
            return View();
        }

        // route: action
        [BindProperty(SupportsGet = true, Name = "action")]
        public string Name { get; set; } // use BindProperty auto get Name = action

        public IActionResult Mercury()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        public IActionResult Venus()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        public IActionResult Earth()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        public IActionResult Mars()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        [HttpGet("/jupiter.html")] // => Only access by GET /jupiter.html
        // [HttpPost("/jupiter.html")] // => Only access by POST /jupiter.html
        public IActionResult Jupiter()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        public IActionResult Saturn()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        public IActionResult Uranus()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        [Route("star/[action]", Order = 1, Name = "neptune1")]                    // => star/Neptune
        [Route("star/[controller]/[action]", Order = 2, Name = "neptune2")]       // => /star/Planet/Neptune
        [Route("[controller]-[action].html", Order = 3, Name = "neptune3")]       // => /Planet-Neptune.html
        // Can use all 3 routes, it take Order to choose main route
        // Add Name to use for View
        public IActionResult Neptune()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        [Route("planets/{id:int}")] // => /planets/1
        public IActionResult PlanetInfo(int id)
        {
            var planet = _planetService.Where(p => p.Id == id).FirstOrDefault();
            return View("Detail", planet);
        }
    }
}