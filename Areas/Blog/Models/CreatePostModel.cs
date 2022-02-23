using System.ComponentModel.DataAnnotations;
using AppMVC.Models.Blog;

namespace AppMVC.Areas.Blog.Models
{
    public class CreatePostModel : Post
    {
        [Display(Name = "Category")]
        public int[] CategoryIds { get; set; }
    }
}