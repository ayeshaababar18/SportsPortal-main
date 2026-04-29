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
    public class SportsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public SportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sports
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            var year = GetSelectedYear(); // Keep GetSelectedYear if it's used elsewhere for other models that have Year
            ViewData["Year"] = year;
            var sports = from s in _context.Sports
                         select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                sports = sports.Where(s => s.SportName.Contains(searchString));
            }

            return View(await sports.ToListAsync());
        }

        // GET: Sports/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sport = await _context.Sports
                .FirstOrDefaultAsync(m => m.SportID == id);
            if (sport == null)
            {
                return NotFound();
            }

            var viewModel = new SportDetailsViewModel
            {
                Sport = sport,
                UpcomingMatches = await _context.Matches
                    .Include(m => m.DepartmentA)
                    .Include(m => m.DepartmentB)
                    .Where(m => m.SportID == id && m.Status != "Finished")
                    .OrderBy(m => m.MatchDate)
                    .ToListAsync(),
                CompletedMatches = await _context.Matches
                    .Include(m => m.DepartmentA)
                    .Include(m => m.DepartmentB)
                    .Where(m => m.SportID == id && m.Status == "Finished")
                    .OrderByDescending(m => m.MatchDate)
                    .ToListAsync(),
                Players = await _context.Players
                    .Include(p => p.Department)
                    .Where(p => p.SportID == id)
                    .OrderBy(p => p.Department.DeptName)
                    .ToListAsync(),
                Teams = await _context.Teams
                    .Include(t => t.Department)
                    .Where(t => t.SportID == id)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Sports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SportID,SportName")] Sport sport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sport);
        }

        // GET: Sports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sport = await _context.Sports.FindAsync(id); // Find by primary key directly
            if (sport == null)
            {
                return NotFound();
            }
            return View(sport);
        }

        // POST: Sports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SportID,SportName")] Sport sport)
        {
            if (id != sport.SportID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SportExists(sport.SportID))
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
            return View(sport);
        }

        // GET: Sports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sport = await _context.Sports
                .FirstOrDefaultAsync(m => m.SportID == id);
            if (sport == null)
            {
                return NotFound();
            }

            return View(sport);
        }

        // POST: Sports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sport = await _context.Sports.FindAsync(id);
            if (sport != null)
            {
                _context.Sports.Remove(sport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SportExists(int id)
        {
            return _context.Sports.Any(e => e.SportID == id);
        }
    }
}
