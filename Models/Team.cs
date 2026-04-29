using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_Teams")]
    public class Team
    {
        [Key]
        [Column("TeamID")]
        public int TeamID { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Team Name can only contain alphabets.")]
        public string TeamName { get; set; } = null!;

        [Column("SportID")]
        public int SportID { get; set; }
        [ForeignKey("SportID")]
        public virtual Sport Sport { get; set; } = null!;

        [Column("DeptID")]
        public int DeptID { get; set; }
        [ForeignKey("DeptID")]
        public virtual Department Department { get; set; } = null!;

        [Required]
        [Column("Category")]
        public TeamCategory Category { get; set; } = TeamCategory.Boys;

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
