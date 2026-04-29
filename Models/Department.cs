using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_Departments")]
    public class Department
    {
        [Key]
        [Column("DeptID")]
        public int DeptID { get; set; }

        [Column("DeptName", TypeName = "nvarchar(50)")]
        [Required]
        [RegularExpression(@"^[a-zA-Z\s&]+$", ErrorMessage = "Department Name can only contain alphabets and '&'.")]
        public string? DeptName { get; set; }

        [Column("LogoUrl", TypeName = "nvarchar(255)")]
        public string? LogoUrl { get; set; } // Nullable as it might not always be present

        [Column("TotalPoints")]
        [Range(0, int.MaxValue, ErrorMessage = "Total Points cannot be negative.")]
        public int TotalPoints { get; set; } = 0;

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
