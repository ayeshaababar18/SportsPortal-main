using System.Collections.Generic;

namespace SportsPortal.Models
{
    public class SearchResultsViewModel
    {
        public string? SearchString { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Match> Matches { get; set; } = new List<Match>();
    }
}
