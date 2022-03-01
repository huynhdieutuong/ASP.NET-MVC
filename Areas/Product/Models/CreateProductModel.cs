using AppMVC.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace AppMVC.Areas.Product.Models
{
    public class CreateProductModel : ProductModel
    {
        [Display(Name = "Category")]
        public int[] CategoryIds { get; set; }
    }
}