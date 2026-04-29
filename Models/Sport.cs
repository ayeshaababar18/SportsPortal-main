using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_Sports")]
    public class Sport
    {
        [Key]
        [Column("SportID")]
        public int SportID { get; set; }

        [Column("SportName", TypeName = "nvarchar(50)")]
        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Sport Name can only contain alphabets.")]
        public string SportName { get; set; } = null!;
    }
}

