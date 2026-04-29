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
    [Authorize(Roles = "Organizer")]
    public class PlayersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public PlayersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Players
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, Gender? gender, int? deptId, int? sportId)
        {
            var year = GetSelectedYear();
            ViewData["Year"] = year;
            
            IQueryable<Player> players = _context.Players.Include(p => p.Department).Include(p => p.Sport);

            if (!String.IsNullOrEmpty(searchString))
            {
                players = players.Where(p => p.FullName.Contains(searchString) ||
                                           (p.RegNumber != null && p.RegNumber.Contains(searchString)));
            }

            if (gender.HasValue)
            {
                players = players.Where(p => p.Gender == gender.Value);
            }

            if (deptId.HasValue)
            {
                players = players.Where(p => p.DeptID == deptId.Value);
            }

            if (sportId.HasValue)
            {
                players = players.Where(p => p.SportID == sportId.Value);
            }

            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", deptId);
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", sportId);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentGender"] = gender;

            return View(await players.ToListAsync());
        }

        // GET: Players/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Department)
                .Include(p => p.Sport)
                .FirstOrDefaultAsync(m => m.PlayerID == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // GET: Players/Create
        public IActionResult Create()
        {
            var year = GetSelectedYear();
            ViewData["Year"] = year;
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName");
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName");
            return View();
        }

        // POST: Players/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlayerID,FullName,RegNumber,DeptID,SportID,TeamID,IsCaptain,Gender")] Player player)
        {
            if (ModelState.IsValid)
            {
                _context.Add(player);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var year = GetSelectedYear();
            ViewData["Year"] = year;
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", player.DeptID);
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", player.SportID);
            ViewData["TeamID"] = new SelectList(_context.Teams.Where(t => t.DeptID == player.DeptID && t.SportID == player.SportID), "TeamID", "TeamName", player.TeamID);
            return View(player);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            var year = GetSelectedYear();
            ViewData["Year"] = year;
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", player.DeptID);
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", player.SportID);
            ViewData["TeamID"] = new SelectList(_context.Teams.Where(t => t.DeptID == player.DeptID && t.SportID == player.SportID), "TeamID", "TeamName", player.TeamID);
            return View(player);
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlayerID,FullName,RegNumber,DeptID,SportID,TeamID,IsCaptain,Gender")] Player player)
        {
            if (id != player.PlayerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(player);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.PlayerID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var year = GetSelectedYear();
            ViewData["Year"] = year;
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", player.DeptID);
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", player.SportID);
            return View(player);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Department)
                .Include(p => p.Sport)
                .FirstOrDefaultAsync(m => m.PlayerID == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.PlayerID == id);
        }

        [HttpGet]
        [Route("api/players/captain")]
        [Authorize(Roles = "Organizer")] // Only organizers can access this
        public async Task<IActionResult> GetCaptain(int deptId, int sportId)
        {
            var year = GetSelectedYear();
            var captain = await _context.Players
                                        .Include(p => p.Department)
                                        .Include(p => p.Sport)
                                        .Where(p => p.DeptID == deptId && p.SportID == sportId && p.IsCaptain == true)
                                        .Select(p => new
                                        {
                                            p.PlayerID,
                                            p.FullName,
                                            p.RegNumber
                                        })
                                        .FirstOrDefaultAsync();

            if (captain == null)
            {
                return Ok(new { captain = (Player?)null });
            }

            return Ok(new { captain = captain });
        }
    }
}
