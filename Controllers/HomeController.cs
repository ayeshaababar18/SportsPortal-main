using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportsPortal.Models;
using SportsPortal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace SportsPortal.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly Services.GeminiService _geminiService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, Services.GeminiService geminiService)
        {
            _logger = logger;
            _context = context;
            _geminiService = geminiService;
        }

        [HttpPost]
        public async Task<IActionResult> AskAI([FromBody] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty.");

            var answer = await _geminiService.GetAIResponse(query);
            return Ok(new { answer });
        }

        public async Task<IActionResult> Index(int? sportId, int? deptId)
    {
        var announcements = await _context.Announcements.OrderByDescending(a => a.PostedDate).ToListAsync();

        var today = DateTime.Today;
        var query = _context.Matches
                                        .Include(m => m.Sport)
                                        .Include(m => m.DepartmentA)
                                        .Include(m => m.DepartmentB)
                                        .Include(m => m.Season)
                                        .Where(m => m.MatchDate.Date == today);

        if (sportId.HasValue)
        {
            query = query.Where(m => m.SportID == sportId.Value);
        }

        if (deptId.HasValue)
        {
            query = query.Where(m => m.DeptA_ID == deptId.Value || m.DeptB_ID == deptId.Value);
        }

        ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", sportId);
        ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", deptId);

        var viewModel = new HomeViewModel
        {
            Announcements = announcements,
            MatchesToday = await query.OrderBy(m => m.MatchDate).ToListAsync()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetLiveMatches()
    {
        var now = DateTime.Now;
        var liveMatches = await _context.Matches
            .Include(m => m.Sport)
            .Include(m => m.DepartmentA)
            .Include(m => m.DepartmentB)
            .Where(m => m.Status == "Live" || (m.Status == "Scheduled" && m.MatchDate > now && m.MatchDate < now.AddHours(24))) // Scheduled within next 24 hours
            .OrderBy(m => m.MatchDate)
            .Select(m => new LiveMatchViewModel
            {
                MatchId = m.MatchID,
                SportName = m.Sport != null ? m.Sport.SportName : "N/A",
                DepartmentA_Name = m.DepartmentA != null ? m.DepartmentA.DeptName : "N/A",
                DepartmentB_Name = m.DepartmentB != null ? m.DepartmentB.DeptName : "N/A",
                ScoreA = m.ScoreA!,
                ScoreB = m.ScoreB!,
                Status = m.Status,
                MatchDateTime = m.MatchDate,
                TimeRemaining = m.Status == "Scheduled" ? FormatTimeRemaining(m.MatchDate - now) : (m.Status == "Live" ? "Live" : "")
            })
            .ToListAsync();

        return Json(liveMatches);
    }

    private static string FormatTimeRemaining(TimeSpan ts)
    {
        if (ts.TotalSeconds <= 0) return "Starting soon";
        if (ts.TotalHours < 1) return $"Starts in {ts.Minutes}m";
        if (ts.TotalDays < 1) return $"Starts in {ts.Hours}h {ts.Minutes}m";
        return $"Starts in {ts.Days}d {ts.Hours}h";
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.Priority == "High")
                .ThenByDescending(a => a.PostedDate)
                .Take(5)
                .Select(a => new {
                    a.AnnouncementID,
                    a.Message,
                    a.Priority,
                    PostedDate = a.PostedDate.ToString("g")
                })
                .ToListAsync();
            return Json(announcements);
        }

        private async Task<int> GetActiveYearAsync()
        {
            if (int.TryParse(HttpContext.Request.Query["year"], out int year))
            {
                return year;
            }
            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsActive);
            return activeSeason?.Year ?? DateTime.Now.Year;
        }

        public async Task<IActionResult> PointsTable()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPointsTableData(TeamCategory? category)
        {
            var year = await GetActiveYearAsync();
            var departments = await _context.Departments.ToListAsync();
            
            // Load matches for the selected year/category to calculate points dynamically
            var query = _context.Matches
                .Include(m => m.Season)
                .Where(m => m.Status == "Finished" && m.Season.Year == year);

            if (category.HasValue)
            {
                query = query.Where(m => m.Category == category.Value);
            }

            var matches = await query.ToListAsync();

            var data = departments.Select(d => {
                int points = 0;
                foreach (var m in matches.Where(m => m.DeptA_ID == d.DeptID || m.DeptB_ID == d.DeptID))
                {
                    int scoreA = 0, scoreB = 0;
                    
                    // Robust Score Parsing
                    if (!string.IsNullOrEmpty(m.ScoreA))
                    {
                        var s = m.ScoreA.Contains("/") ? m.ScoreA.Split('/')[0] : m.ScoreA;
                        int.TryParse(s, out scoreA);
                    }
                    
                    if (!string.IsNullOrEmpty(m.ScoreB))
                    {
                        var s = m.ScoreB.Contains("/") ? m.ScoreB.Split('/')[0] : m.ScoreB;
                        int.TryParse(s, out scoreB);
                    }

                    if (m.DeptA_ID == d.DeptID)
                    {
                        if (scoreA > scoreB) points += 2;
                        else if (scoreA == scoreB) points += 1;
                    }
                    else
                    {
                        if (scoreB > scoreA) points += 2;
                        else if (scoreA == scoreB) points += 1;
                    }
                }
                return new {
                    deptID = d.DeptID,
                    deptName = d.DeptName,
                    logoUrl = d.LogoUrl,
                    totalPoints = points
                };
            }).OrderByDescending(x => x.totalPoints).ToList();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartmentMatchDetails(int id, TeamCategory? category)
        {
            var year = await GetActiveYearAsync();
            
            var query = _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.DepartmentA)
                .Include(m => m.DepartmentB)
                .Include(m => m.Season)
                .Where(m => (m.DeptA_ID == id || m.DeptB_ID == id) 
                            && m.Status == "Finished"
                            && m.Season.Year == year);

            if (category.HasValue)
            {
                query = query.Where(m => m.Category == category.Value);
            }

            var matches = await query.OrderByDescending(m => m.MatchDate).ToListAsync();

            var matchDetails = matches.Select(m => {
                int scoreA = 0;
                int scoreB = 0;
                int pointsEarned = 0;
                string result = "Loss";

                // Parse Scores safely
                if (!string.IsNullOrEmpty(m.ScoreA))
                {
                    var s = m.ScoreA.Contains("/") ? m.ScoreA.Split('/')[0] : m.ScoreA;
                    int.TryParse(s, out scoreA);
                }
                
                if (!string.IsNullOrEmpty(m.ScoreB))
                {
                    var s = m.ScoreB.Contains("/") ? m.ScoreB.Split('/')[0] : m.ScoreB;
                    int.TryParse(s, out scoreB);
                }

                // Determine Result and Points
                if (m.DeptA_ID == id)
                {
                    if (scoreA > scoreB) { pointsEarned = 2; result = "Win"; }
                    else if (scoreA == scoreB) { pointsEarned = 1; result = "Draw"; }
                }
                else
                {
                    if (scoreB > scoreA) { pointsEarned = 2; result = "Win"; }
                    else if (scoreA == scoreB) { pointsEarned = 1; result = "Draw"; }
                }

                return new 
                {
                    matchDate = m.MatchDate.ToString("MMM dd, yyyy"),
                    sportName = m.Sport?.SportName ?? "Unknown",
                    category = m.Category.ToString(),
                    opponent = m.DeptA_ID == id ? m.DepartmentB?.DeptName : m.DepartmentA?.DeptName,
                    score = $"{m.ScoreA} - {m.ScoreB}",
                    result = result,
                    points = pointsEarned
                };
            }).ToList();

            return Json(matchDetails);
        }

        // GET: Home/ScheduledMatches
        public async Task<IActionResult> ScheduledMatches(int? sportId, int? deptId)
        {
            IQueryable<Match> query = _context.Matches
                                                .Include(m => m.Sport)
                                                .Include(m => m.DepartmentA)
                                                .Include(m => m.DepartmentB)
                                                .Include(m => m.Season)
                                                .Where(m => m.Status == "Scheduled");

            if (sportId.HasValue)
            {
                query = query.Where(m => m.SportID == sportId.Value);
            }

            if (deptId.HasValue)
            {
                query = query.Where(m => m.DeptA_ID == deptId.Value || m.DeptB_ID == deptId.Value);
            }

            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", sportId);
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", deptId);

            var scheduledMatches = await query.OrderBy(m => m.MatchDate).ToListAsync();
            return View(scheduledMatches);
        }
    }
}
