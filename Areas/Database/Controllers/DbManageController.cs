using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AppMVC.Areas.Database.Controllers
{
    public class DbManageController : Controller
    {
        [Area("Database")]
        [Route("/database-manage/[action]")]
        public IActionResult Index()
        {
            return View();
        }
    }
}