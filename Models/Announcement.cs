using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_Announcements")]
    public class Announcement
    {
        [Key]
        [Column("AnnouncementID")]
        public int AnnouncementID { get; set; }

        [Column("Message", TypeName = "nvarchar(MAX)")] // Assuming 'Text' maps to nvarchar(MAX)
        [Required]
        [MinLength(5, ErrorMessage = "Message must be at least 5 characters long.")]
        public string? Message { get; set; }

        [Column("Priority", TypeName = "varchar(20)")]
        [Required]
        [RegularExpression("^(High|Normal)$", ErrorMessage = "Priority must be either 'High' or 'Normal'.")]
        public string? Priority { get; set; } // 'High' or 'Normal'

        [Column("PostedDate")]
        public DateTime PostedDate { get; set; } = DateTime.Now;
    }
}

