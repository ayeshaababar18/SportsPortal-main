using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsPortal.Models
{
    [Table("tbl_AdminUsers")]
    public class AdminUser
    {
        [Key]
        [Column("AdminID")]
        public int AdminID { get; set; }

        [Column("Username", TypeName = "nvarchar(50)")]
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers.")]
        public string? Username { get; set; }

        [Column("Password", TypeName = "nvarchar(255)")]
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [Column("Role", TypeName = "nvarchar(20)")]
        [Required]
        [RegularExpression("^(Organizer)$", ErrorMessage = "Role must be 'Organizer'.")]
        public string? Role { get; set; } // e.g., 'Organizer'
    }
}
