using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsPortal.Data;
using SportsPortal.Models;
using Microsoft.AspNetCore.Authorization; // Add this line

namespace SportsPortal.Controllers
{
    [Authorize(Roles = "Organizer")] // Require Organizer role for all actions by default
    public class MatchesController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public MatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Matches
        [AllowAnonymous] // Allow public access to view the list of matches
        public async Task<IActionResult> Index(string searchString, TeamCategory? category, int? sportId, int? deptId, string status)
        {
            var year = GetSelectedYear();
            ViewData["Year"] = year;

            // Filter matches by the selected year through the Season.
            IQueryable<Match> query = _context.Matches
                                                .Include(m => m.Sport)
                                                .Include(m => m.DepartmentA).ThenInclude(d => d.Players)
                                                .Include(m => m.DepartmentB).ThenInclude(d => d.Players)
                                                .Include(m => m.Season)
                                                .Where(m => m.Season.Year == year)
                                                .AsSplitQuery(); // Optimize query performance

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(m => m.Sport.SportName.Contains(searchString));
            }

            if (category.HasValue)
            {
                query = query.Where(m => m.Category == category.Value);
            }

            if (sportId.HasValue)
            {
                query = query.Where(m => m.SportID == sportId.Value);
            }

            if (deptId.HasValue)
            {
                query = query.Where(m => m.DeptA_ID == deptId.Value || m.DeptB_ID == deptId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(m => m.Status == status);
            }

            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", sportId);
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", deptId);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = category;
            ViewData["CurrentStatus"] = status;

            return View(await query.OrderByDescending(m => m.MatchDate).ToListAsync());
        }

        // GET: Matches/Details/5
        [AllowAnonymous] // Allow public access to view match details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.DepartmentA)
                .Include(m => m.DepartmentB)
                .Include(m => m.Season)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // GET: Matches/Create
        public IActionResult Create()
        {
            var year = GetSelectedYear();
            ViewData["Year"] = year;

            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName");
            ViewData["DeptA_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName");
            ViewData["DeptB_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName");
            ViewData["SeasonID"] = new SelectList(_context.Seasons.Where(s => s.Year == year), "SeasonID", "Year");
            return View();
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MatchID,SportID,DeptA_ID,DeptB_ID,ScoreA,ScoreB,Status,MatchDate,SeasonID,Category")] Match match)
        {
            Console.WriteLine("Create POST action reached.");
            if (ModelState.IsValid)
            {
                Console.WriteLine("Model state is valid. Saving match...");
                try
                {
                    _context.Add(match);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Match saved successfully.");
                    return RedirectToAction(nameof(Index), new { year = _context.Seasons.Find(match.SeasonID)?.Year });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving match: {ex.Message}");
                    if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            else
            {
                Console.WriteLine("Model state is INVALID.");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            var year = GetSelectedYear();
            ViewData["Year"] = year;
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", match.SportID);
            ViewData["DeptA_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName", match.DeptA_ID);
            ViewData["DeptB_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName", match.DeptB_ID);
            ViewData["SeasonID"] = new SelectList(_context.Seasons.Where(s => s.Year == year), "SeasonID", "Year", match.SeasonID);
            return View(match);
        }

        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            var year = GetSelectedYear();
            ViewData["Year"] = year;
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", match.SportID);
            ViewData["DeptA_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName", match.DeptA_ID);
            ViewData["DeptB_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName", match.DeptB_ID);
            ViewData["SeasonID"] = new SelectList(_context.Seasons.Where(s => s.Year == year), "SeasonID", "Year", match.SeasonID);
            return View(match);
        }

        // POST: Matches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MatchID,SportID,DeptA_ID,DeptB_ID,ScoreA,ScoreB,Status,MatchDate,SeasonID,Category")] Match match)
        {
            if (id != match.MatchID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();
                    await UpdatePoints(match);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.MatchID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { year = _context.Seasons.Find(match.SeasonID)?.Year });
            }
            var year = GetSelectedYear();
            ViewData["Year"] = year;
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", match.SportID);
            ViewData["DeptA_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName", match.DeptA_ID);
            ViewData["DeptB_ID"] = new SelectList(_context.Departments, "DeptID", "DeptName", match.DeptB_ID);
            ViewData["SeasonID"] = new SelectList(_context.Seasons.Where(s => s.Year == year), "SeasonID", "Year", match.SeasonID);
            return View(match);
        }

        // POST: Matches/UpdateScore (API endpoint for AJAX updates)
        [HttpPost]
        [Route("api/matches/updateScore")]
        [Authorize(Roles = "Organizer")] // Only organizers can update scores
        public async Task<IActionResult> UpdateScore([FromBody] UpdateScoreRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var match = await _context.Matches.FindAsync(request.MatchID);

            if (match == null)
            {
                return NotFound(new { success = false, message = "Match not found." });
            }

            // Capture old status to check for state transitions if needed
            var oldStatus = match.Status;

            match.ScoreA = request.ScoreA;
            match.ScoreB = request.ScoreB;
            match.Status = request.Status;

            try
            {
                _context.Update(match);
                
                // If the match is now finished, update the points.
                // Note: Real idempotency requires a flag on the match or checking a 'PointsAwarded' table.
                // For this current architecture, we assume 'Finished' status triggers the update.
                // Ideally, we should recalculate points from scratch for the season to be perfectly safe,
                // or have a flag. For now, we call UpdatePoints which adds points.
                // WARNING: Calling this multiple times on a finished match will add points multiple times
                // with the current implementation. To fix this properly, we need to RecalculatePoints
                // for the whole department or season, OR add a 'PointsAwarded' boolean to the Match model.
                // Given the constraints, we will call RecalculatePoints for the involved departments
                // to ensure accuracy without adding new columns.
                
                await _context.SaveChangesAsync();
                
                if (match.Status == "Finished")
                {
                     // Use RecalculatePoints approach to be safe and idempotent
                     await RecalculatePointsForDepartment(match.DeptA_ID);
                     await RecalculatePointsForDepartment(match.DeptB_ID);
                }

                return Ok(new { success = true, message = "Score updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MatchExists(request.MatchID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: Matches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.DepartmentA)
                .Include(m => m.DepartmentB)
                .Include(m => m.Season)
                .FirstOrDefaultAsync(m => m.MatchID == id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match != null)
            {
                _context.Matches.Remove(match);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Replaced UpdatePoints with a safer Recalculate strategy for the specific departments involved
        private async Task UpdatePoints(Match match)
        {
             // Instead of adding points incrementally (which is prone to errors if edited multiple times),
             // we recalculate the total points for the affected departments from scratch.
             if (match.DeptA_ID.HasValue) await RecalculatePointsForDepartment(match.DeptA_ID.Value);
             if (match.DeptB_ID.HasValue) await RecalculatePointsForDepartment(match.DeptB_ID.Value);
        }

        private async Task RecalculatePointsForDepartment(int? deptId)
        {
            if (!deptId.HasValue) return;

            var department = await _context.Departments.FindAsync(deptId);
            if (department == null) return;

            var year = GetSelectedYear();
            
            // Find all finished matches for this department in the current season
            var matches = await _context.Matches
                .Include(m => m.Season)
                .Where(m => (m.DeptA_ID == deptId || m.DeptB_ID == deptId) 
                            && m.Status == "Finished"
                            && m.Season.Year == year)
                .ToListAsync();

            int totalPoints = 0;

            foreach (var m in matches)
            {
                int scoreA = 0;
                int scoreB = 0;

                // Simple parsing
                if (int.TryParse(m.ScoreA, out int sA)) scoreA = sA;
                else if (!string.IsNullOrEmpty(m.ScoreA) && m.ScoreA.Contains("/")) int.TryParse(m.ScoreA.Split('/')[0], out scoreA);

                if (int.TryParse(m.ScoreB, out int sB)) scoreB = sB;
                else if (!string.IsNullOrEmpty(m.ScoreB) && m.ScoreB.Contains("/")) int.TryParse(m.ScoreB.Split('/')[0], out scoreB);

                if (m.DeptA_ID == deptId)
                {
                    if (scoreA > scoreB) totalPoints += 2; // Win
                    else if (scoreA == scoreB) totalPoints += 1; // Draw
                }
                else if (m.DeptB_ID == deptId)
                {
                    if (scoreB > scoreA) totalPoints += 2; // Win
                    else if (scoreA == scoreB) totalPoints += 1; // Draw
                }
            }

            department.TotalPoints = totalPoints;
            _context.Update(department);
            await _context.SaveChangesAsync();
        }


        private bool MatchExists(int id)
        {
            return _context.Matches.Any(e => e.MatchID == id);
        }

    }
}
