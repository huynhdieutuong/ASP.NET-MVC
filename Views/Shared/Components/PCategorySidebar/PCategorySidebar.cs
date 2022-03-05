using System.Collections.Generic;
using AppMVC.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace AppMVC.Components
{
    [ViewComponent]
    public class PCategorySidebar : ViewComponent
    {
        public class CategorySidebarData
        {
            public List<PCategory> Categories { get; set; }
            public int Level { get; set; }
            public string CategorySlug { get; set; }
        }

        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
    }
}