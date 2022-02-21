using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Blog
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} must be between {2} - {1} chars")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Current url", Prompt = "If slug isn't entered, it will be generated based on Title")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} must be between {2} - {1} chars")]
        [RegularExpression("^[a-z0-9-]*$", ErrorMessage = "Slug only contains [a-z] or [0-9]")]
        public string Slug { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }

        [Required]
        [Display(Name = "Author")]
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        [Display(Name = "Author")]
        public AppUser Author { get; set; }

        public List<PostCategory> PostCategories { get; set; }

        [Display(Name = "Date created")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Date updated")]
        public DateTime DateUpdated { get; set; }
    }
}