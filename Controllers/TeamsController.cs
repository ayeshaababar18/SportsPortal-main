using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsPortal.Data;
using SportsPortal.Models;

namespace SportsPortal.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class TeamsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teams
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? sportId, int? deptId)
        {
            IQueryable<Team> teams = _context.Teams
                                    .Include(t => t.Sport)
                                    .Include(t => t.Department);

            if (sportId.HasValue)
            {
                teams = teams.Where(t => t.SportID == sportId.Value);
            }

            if (deptId.HasValue)
            {
                teams = teams.Where(t => t.DeptID == deptId.Value);
            }

            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", sportId);
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", deptId);

            return View(await teams.ToListAsync());
        }

        // GET: Teams/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Sport)
                .Include(t => t.Department)
                .Include(t => t.Players)
                .FirstOrDefaultAsync(m => m.TeamID == id);
            
            if (team == null)
            {
                return NotFound();
            }

            var viewModel = new TeamDetailsViewModel
            {
                Team = team,
                Players = team.Players.ToList()
            };

            return View(viewModel);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName");
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName");
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamID,TeamName,SportID,DeptID,Category")] Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", team.DeptID);
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", team.SportID);
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", team.DeptID);
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", team.SportID);
            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamID,TeamName,SportID,DeptID,Category")] Team team)
        {
            if (id != team.TeamID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.TeamID))
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
            ViewData["DeptID"] = new SelectList(_context.Departments, "DeptID", "DeptName", team.DeptID);
            ViewData["SportID"] = new SelectList(_context.Sports, "SportID", "SportName", team.SportID);
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Sport)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(m => m.TeamID == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamID == id);
        }
    }
}
