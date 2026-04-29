using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_GalleryImages")]
    public class GalleryImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Title can only contain alphabets.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Category can only contain alphabets.")]
        public string Category { get; set; } = "General"; // e.g., Futsal, Cricket, Ceremony

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
