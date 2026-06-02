using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebLibrary.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        [Required]
        public string Isbn { get; set; }
        [Required]
        public string Author { get; set; }

        [Required]
        [DisplayName("List price")]
        [Range(1, 1000)]
        public int ListPrice { get; set; }
        [Required]
        [Display(Name = "Price")]
        [Range(1, 1000)]
        public double Price { get; set; }

        public int CategoryId { get; set; }
         
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        public string? ImageUrl { get; set; }

    }
}
