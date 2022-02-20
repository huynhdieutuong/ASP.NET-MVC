using Microsoft.AspNetCore.Mvc;

namespace CategoryComponents
{
    [ViewComponent]
    public class TreeCategory : ViewComponent
    {
        public TreeCategory() { }

        public IViewComponentResult Invoke(dynamic data)
        {
            return View("TreeCategory", data);
        }
    }
}