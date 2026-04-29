using System.Collections.Generic;

namespace SportsPortal.Models
{
    public class TeamDetailsViewModel
    {
        public Team Team { get; set; } = null!;
        public List<Player> Players { get; set; } = new List<Player>();
    }
}
