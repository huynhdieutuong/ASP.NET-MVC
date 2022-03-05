using Microsoft.AspNetCore.Mvc;

namespace AppMVC.Components
{
    [ViewComponent]
    public class BreadCrumb : ViewComponent
    {
        public IViewComponentResult Invoke(dynamic list)
        {
            return View(list);
        }
    }
}