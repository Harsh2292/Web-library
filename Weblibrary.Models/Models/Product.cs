using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Price for 1-50")]
        [Range(1, 1000)]
        public double Price { get; set; }

    }
}
