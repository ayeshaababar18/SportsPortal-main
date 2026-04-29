using System;
using System.Collections.Generic;

namespace SportsPortal.Models
{
    public class PointsRecalculationViewModel
    {
        public List<DepartmentPointsReport> DepartmentReports { get; set; } = new List<DepartmentPointsReport>();
    }

    public class DepartmentPointsReport
    {
        public string? DeptName { get; set; }
        public int TotalPoints { get; set; }
        public List<MatchPointDetail> MatchDetails { get; set; } = new List<MatchPointDetail>();
    }

    public class MatchPointDetail
    {
        public DateTime MatchDate { get; set; }
        public string? SportName { get; set; }
        public string? OpponentName { get; set; }
        public string? ScoreResult { get; set; } // e.g., "150/5 vs 140/8"
        public int PointsAwarded { get; set; }
        public string? Outcome { get; set; } // "Win", "Loss", "Draw"
    }
}
