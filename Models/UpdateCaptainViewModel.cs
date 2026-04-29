using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SportsPortal.Models
{
    public class UpdateCaptainViewModel
    {
        public int SelectedDepartmentId { get; set; }
        public IEnumerable<SelectListItem>? Departments { get; set; }

        public int SelectedSportId { get; set; }
        public IEnumerable<SelectListItem>? Sports { get; set; }

        public int? CurrentCaptainPlayerId { get; set; } // Player ID of the current captain, if any
        public string? CurrentCaptainName { get; set; } // Name of the current captain

        [Required(ErrorMessage = "Please enter the captain's full name.")]
        public string? NewCaptainFullName { get; set; }
        public int? NewCaptainRegNumber { get; set; }
    }
}
