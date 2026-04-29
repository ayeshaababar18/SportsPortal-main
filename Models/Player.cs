using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_Players")]
    public class Player
    {
        [Key]
        [Column("PlayerID")]
        public int PlayerID { get; set; }

        [Column("FullName", TypeName = "nvarchar(100)")]
        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain alphabets.")]
        public string? FullName { get; set; }

        [Column("RegNumber", TypeName = "nvarchar(50)")]
        [RegularExpression(@"^[a-zA-Z0-9-]+$", ErrorMessage = "Registration Number can only contain letters, numbers and hyphens.")]
        public string? RegNumber { get; set; } // Nullable

        [Column("DeptID")]
        public int DeptID { get; set; }
        [ForeignKey("DeptID")]
        public virtual Department Department { get; set; } = null!;

        [Column("SportID")]
        public int SportID { get; set; }
        [ForeignKey("SportID")]
        public virtual Sport Sport { get; set; } = null!;

        [Column("TeamID")]
        public int? TeamID { get; set; }
        [ForeignKey("TeamID")]
        public virtual Team? Team { get; set; }

        [Column("Gender")]
        public Gender Gender { get; set; } = Gender.Male;

        [Column("IsCaptain")]
        public bool IsCaptain { get; set; } = false;
    }
}

