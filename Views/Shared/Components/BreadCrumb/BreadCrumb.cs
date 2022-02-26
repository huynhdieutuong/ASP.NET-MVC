using System.Collections.Generic;
using AppMVC.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace AppMVC.Components
{
    [ViewComponent]
    public class BreadCrumb : ViewComponent
    {
        public IViewComponentResult Invoke(List<Category> list)
        {
            return View(list);
        }
    }
}