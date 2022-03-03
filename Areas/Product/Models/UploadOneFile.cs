using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AppMVC.Areas.Product.Models
{
    public class UploadOneFile
    {
        [Required(ErrorMessage = "Please select a file")]
        [FileExtensions(Extensions = "png, jpg, jpeg, gif")]
        [DataType(DataType.Upload)]
        [Display(Name = "Select one file to upload")]
        public IFormFile FileUpload { get; set; }
    }
}