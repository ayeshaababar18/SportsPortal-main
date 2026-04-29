using System.Collections.Generic;

namespace SportsPortal.Models
{
    public class SportDetailsViewModel
    {
        public Sport Sport { get; set; } = null!;
        public List<Match> UpcomingMatches { get; set; } = new List<Match>();
        public List<Match> CompletedMatches { get; set; } = new List<Match>();
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Team> Teams { get; set; } = new List<Team>();
    }
}
