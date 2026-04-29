using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_Seasons")]
    public class Season
    {
        [Key]
        [Column("SeasonID")]
        public int SeasonID { get; set; }

        [Column("Year")]
        [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100.")]
        public int Year { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = false; // Set '1' for current year.
    }
}
