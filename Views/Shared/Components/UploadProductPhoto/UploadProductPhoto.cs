using AppMVC.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace AppMVC.Components
{
    [ViewComponent]
    public class UploadProductPhoto : ViewComponent
    {
        public IViewComponentResult Invoke(dynamic productId)
        {
            return View(productId);
        }
    }
}