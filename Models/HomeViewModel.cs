using System.Collections.Generic;

namespace SportsPortal.Models
{
    public class HomeViewModel
    {
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();
        public List<Match> MatchesToday { get; set; } = new List<Match>();
    }
}