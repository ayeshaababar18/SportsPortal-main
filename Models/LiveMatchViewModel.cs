using System;

namespace SportsPortal.Models
{
    public class LiveMatchViewModel
    {
        public int MatchId { get; set; }
        public string? SportName { get; set; }
        public string? DepartmentA_Name { get; set; }
        public string? DepartmentB_Name { get; set; }
        public string? ScoreA { get; set; }
        public string? ScoreB { get; set; }
        public string? Status { get; set; }
        public DateTime MatchDateTime { get; set; }
        public string? TimeRemaining { get; set; } // e.g., "Starts in 1h 30m" or "Live"
    }
}
