using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Contacts
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Please input a valid email")]
        public string Email { get; set; }

        public DateTime DateSent { get; set; }

        public string Message { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Please input a valid phone number")]
        [Display(Name = "Phone number")]
        public string Phone { get; set; }
    }
}