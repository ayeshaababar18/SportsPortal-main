using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsPortal.Data;
using SportsPortal.Models;

namespace SportsPortal.Controllers
{
    [Authorize(Roles = "Organizer")]
    public class DepartmentsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments (Public View)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                                            .OrderByDescending(d => d.TotalPoints)
                                            .ToListAsync();
            return View(departments);
        }

        // GET: Departments/Manage (Admin View)
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Manage()
        {
            var departments = await _context.Departments
                                            .OrderByDescending(d => d.TotalPoints)
                                            .ToListAsync();
            return View("Manage", departments);
        }

        // GET: Departments/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // var year = GetSelectedYear(); // Removed unused variable

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DeptID == id);

            if (department == null)
            {
                return NotFound();
            }

            // Calculate rank
            var allDepartments = await _context.Departments.OrderByDescending(d => d.TotalPoints).ToListAsync();
            var rank = allDepartments.FindIndex(d => d.DeptID == department.DeptID) + 1;

            // Get matches played by this department (All history)
            var departmentMatches = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.DepartmentA)
                .Include(m => m.DepartmentB)
                .Include(m => m.Season)
                .Where(m => m.DeptA_ID == id || m.DeptB_ID == id)
                .OrderByDescending(m => m.MatchDate)
                .ToListAsync();

            // Get all players for this department
            var allPlayers = await _context.Players
                .Where(p => p.DeptID == id)
                .Include(p => p.Sport)
                .ToListAsync();

            // Group by Sport and filter out any players without a sport assigned
            var playersBySport = allPlayers
                .Where(p => p.Sport != null)
                .GroupBy(p => p.Sport!, new SportEqualityComparer())
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new DepartmentDetailsViewModel
            {
                Department = department,
                Matches = departmentMatches,
                Rank = rank,
                PlayersBySport = playersBySport
            };

            return View(viewModel);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeptID,DeptName,LogoUrl,TotalPoints")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeptID,DeptName,LogoUrl,TotalPoints")] Department department)
        {
            if (id != department.DeptID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DeptID))
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
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DeptID == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DeptID == id);
        }
    }
}