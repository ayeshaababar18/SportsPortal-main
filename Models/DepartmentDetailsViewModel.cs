using System.Collections.Generic;
using SportsPortal.Models;

namespace SportsPortal.Models
{
    public class DepartmentDetailsViewModel
    {
        public Department Department { get; set; } = null!;
        public IEnumerable<Match> Matches { get; set; } = new List<Match>();
        public int Rank { get; set; }
        public Dictionary<Sport, List<Player>> PlayersBySport { get; set; } = new Dictionary<Sport, List<Player>>();
    }
}
