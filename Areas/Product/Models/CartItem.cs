using AppMVC.Models.Product;

namespace AppMVC.Areas.Product.Models
{
    public class CartItem
    {
        public int Quantity { get; set; }
        public ProductModel Product { get; set; }
    }
}
