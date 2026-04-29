using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportsPortal.Models.Validation;

namespace SportsPortal.Models
{
    [Table("tbl_Matches")]
    public class Match
    {
        [Key]
        [Column("MatchID")]
        public int MatchID { get; set; }

        [Column("SportID")]
        public int SportID { get; set; }
        [ForeignKey("SportID")]
        public virtual Sport? Sport { get; set; }

        [Column("DeptA_ID")] // Changed to DeptA_ID
        public int? DeptA_ID { get; set; }
        [ForeignKey("DeptA_ID")]
        public virtual Department? DepartmentA { get; set; } // Changed to DepartmentA

        [Column("DeptB_ID")] // Changed to DeptB_ID
        public int? DeptB_ID { get; set; }
        [ForeignKey("DeptB_ID")]
        public virtual Department? DepartmentB { get; set; } // Changed to DepartmentB

        [Column("ScoreA", TypeName = "nvarchar(50)")]
        [ScoreValidation]
        public string? ScoreA { get; set; }

        [Column("ScoreB", TypeName = "nvarchar(50)")]
        [ScoreValidation]
        public string? ScoreB { get; set; }

        [Column("Status", TypeName = "nvarchar(20)")]
        [Required]
        [RegularExpression("^(Scheduled|Live|Finished)$", ErrorMessage = "Status must be 'Scheduled', 'Live', or 'Finished'.")]
        public string Status { get; set; } = "Scheduled"; // 'Scheduled', 'Live', 'Finished'

        [Column("MatchDate")]
        public DateTime MatchDate { get; set; }

        [Column("SeasonID")] // Added SeasonID
        public int SeasonID { get; set; }
        [ForeignKey("SeasonID")]
        public virtual Season? Season { get; set; }

        [Column("Category")]
        public TeamCategory Category { get; set; } = TeamCategory.Boys;
    }

}
