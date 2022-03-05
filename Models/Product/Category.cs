using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Product
{
    public class PCategory
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

        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} must be between {2} - {1} chars")]
        [RegularExpression("^[a-z0-9-]*$", ErrorMessage = "Only accept [a-z] or [0-9] chars")]
        [Display(Name = "Current url", Prompt = "If slug isn't entered, it will be generated based on Title")]
        public string Slug { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        [Display(Name = "Parent category")]
        public PCategory ParentCategory { get; set; }

        public ICollection<PCategory> ChildrenCategories { get; set; }

        public List<PCategory> GetChildrenList()
        {
            PCategory category = this;
            List<PCategory> childrenList = new List<PCategory>();
            AddChildren(childrenList, category);
            return childrenList;
        }

        private void AddChildren(List<PCategory> childrenList, PCategory category)
        {
            if (category.ChildrenCategories?.Count > 0)
            {
                foreach (PCategory child in category.ChildrenCategories)
                {
                    childrenList.Add(child);
                    AddChildren(childrenList, child);
                }
            }
        }

        public List<PCategory> GetParentList()
        {
            List<PCategory> parentsList = new List<PCategory>();
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