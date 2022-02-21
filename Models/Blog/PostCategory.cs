using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Blog
{
    [Table("PostCategory")]
    public class PostCategory
    {
        public int PostId { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}