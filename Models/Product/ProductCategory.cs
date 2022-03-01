using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Product
{
    [Table("ProductCategory")]
    public class ProductCategory
    {
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public PCategory Category { get; set; }
    }
}
