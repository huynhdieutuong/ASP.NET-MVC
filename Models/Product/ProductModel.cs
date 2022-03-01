using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Product
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} must be between {2} - {1} chars")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.Text)]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [RegularExpression("^[a-z0-9-]*$", ErrorMessage = "Slug only contains [a-z] or [0-9]")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} must be between {2} - {1} chars")]
        [Display(Name = "Current url", Prompt = "If slug isn't entered, it will be generated based on Title")]
        public string Slug { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} must be between {1} - {2}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public AppUser Author { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }

        [Display(Name = "Date created")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Date updated")]
        public DateTime DateUpdated { get; set; }

        public List<ProductCategory> ProductCategories { get; set; }
    }
}
