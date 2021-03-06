using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Blog
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} must be between {2} - {1} chars")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.Text)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} must be between {2} - {1} chars")]
        [RegularExpression("^[a-z0-9-]*$", ErrorMessage = "Only accept [a-z] or [0-9] chars")]
        [Display(Name = "Current url")]
        public string Slug { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        [Display(Name = "Parent category")]
        public Category ParentCategory { get; set; }

        [Display(Name = "Children category")]
        public ICollection<Category> ChildrenCategory { get; set; }

        public void GetChildIds(ICollection<Category> childCats, List<int> catIds)
        {
            if (childCats?.Count > 0)
            {
                foreach (var childCat in childCats)
                {
                    catIds.Add(childCat.Id);
                    GetChildIds(childCat.ChildrenCategory, catIds);
                }
            }
        }

        public List<Category> GetParentsList()
        {
            List<Category> parentsList = new List<Category>();
            parentsList.Add(this);

            var parent = this.ParentCategory;
            while (parent != null)
            {
                parentsList.Add(parent);
                parent = parent.ParentCategory;
            }

            parentsList.Reverse();
            return parentsList;
        }
    }
}