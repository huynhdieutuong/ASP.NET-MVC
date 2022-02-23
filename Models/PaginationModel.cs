using System;

namespace AppMVC.Models
{
    public class PaginationModel
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageRange { get; set; }
        public Func<int?, string> GenerateUrl { get; set; }
    }
}