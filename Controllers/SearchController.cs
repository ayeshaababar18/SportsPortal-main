using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsPortal.Data;
using SportsPortal.Models;

namespace SportsPortal.Controllers
{
    public class SearchController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Search
        public async Task<IActionResult> Index(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(new SearchResultsViewModel()); // Return empty model if no search string
            }

            var year = GetSelectedYear();

            // Search in Players
            var players = await _context.Players
                                        .Include(p => p.Department)
                                        .Include(p => p.Sport)
                                        .Where(p => p.FullName.Contains(searchString) ||
                                                    (p.RegNumber != null && p.RegNumber.Contains(searchString)))
                                        .ToListAsync();

            // Search in Matches (considering departments, sport names, scores)
            var matchesByName = await _context.Matches
                                        .Include(m => m.Sport)
                                        .Include(m => m.DepartmentA)
                                        .Include(m => m.DepartmentB)
                                        .Where(m => (m.Sport != null && m.Sport.SportName.Contains(searchString)) ||
                                                    (m.DepartmentA != null && m.DepartmentA.DeptName.Contains(searchString)) ||
                                                    (m.DepartmentB != null && m.DepartmentB.DeptName.Contains(searchString)) ||
                                                    (m.ScoreA != null && m.ScoreA.Contains(searchString)) ||
                                                    (m.ScoreB != null && m.ScoreB.Contains(searchString)) &&
                                                    m.Season.Year == year)
                                        .ToListAsync();

            // Find departments that have players matching the search string
            var departmentIdsWithMatchingPlayers = await _context.Players
                .Where(p => p.FullName.Contains(searchString))
                .Select(p => p.DeptID)
                .Distinct()
                .ToListAsync();

            // Find matches involving those departments
            var matchesByPlayer = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.DepartmentA)
                .Include(m => m.DepartmentB)
                .Where(m => (m.DeptA_ID.HasValue && departmentIdsWithMatchingPlayers.Contains(m.DeptA_ID.Value) || 
                               (m.DeptB_ID.HasValue && departmentIdsWithMatchingPlayers.Contains(m.DeptB_ID.Value))) && m.Season.Year == year)
                .ToListAsync();

            var combinedMatches = matchesByName.Union(matchesByPlayer).ToList();

            var viewModel = new SearchResultsViewModel
            {
                SearchString = searchString,
                Players = players,
                Matches = combinedMatches
            };

            return View(viewModel);
        }
    }
}
