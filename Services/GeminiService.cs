using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SportsPortal.Data;

namespace SportsPortal.Services
{
    public class GeminiService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GeminiService(ApplicationDbContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> GetAIResponse(string userQuery)
        {
            var apiKey = _configuration["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return "Error: API Key is missing.";
            }

            // 1. Gather Context from DB
            var contextData = await BuildContextData();

            // 2. Construct Prompt
            var prompt = $@"
You are an AI assistant for a University Sports Week event. 
Answer the user's question using ONLY the data provided below. 
If the answer is not in the data, state that you don't have that information.
Do not invent facts. Be concise and friendly.

=== EVENT DATA ===
{contextData}
==================

User Question: {userQuery}
Answer:
";

            // 3. Call Gemini API
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-lite-latest:generateContent?key={apiKey}";

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Error from AI: {response.StatusCode} - {responseString}";
                }

                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "No response generated.";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        private async Task<string> BuildContextData()
        {
            var sb = new StringBuilder();

            // Departments & Points
            var departments = await _context.Departments.OrderByDescending(d => d.TotalPoints).ToListAsync();
            sb.AppendLine("Current Standings (Departments):");
            foreach (var d in departments)
            {
                sb.AppendLine($"- {d.DeptName}: {d.TotalPoints} points");
            }
            sb.AppendLine();

            // Upcoming/Live/Recent Matches
            var matches = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.DepartmentA)
                .Include(m => m.DepartmentB)
                .OrderByDescending(m => m.MatchDate)
                .Take(50) // Limit to 50 recent/upcoming to save tokens
                .ToListAsync();

            sb.AppendLine("Matches Schedule & Results:");
            foreach (var m in matches)
            {
                var deptA = m.DepartmentA?.DeptName ?? "TBD";
                var deptB = m.DepartmentB?.DeptName ?? "TBD";
                var score = (m.Status == "Finished") ? $" (Score: {m.ScoreA} - {m.ScoreB})" : "";
                sb.AppendLine($"- {m.MatchDate:g}: {m.Sport.SportName} - {deptA} vs {deptB} [{m.Status}]{score}");
            }
            sb.AppendLine();

            // Announcements
            var announcements = await _context.Announcements.OrderByDescending(a => a.PostedDate).Take(10).ToListAsync();
            sb.AppendLine("Recent Announcements:");
            foreach (var a in announcements)
            {
                sb.AppendLine($"- [{a.Priority}] {a.PostedDate.ToShortDateString()}: {a.Message}");
            }
            sb.AppendLine();

            // Team Captains & Rosters
            var players = await _context.Players
                .Include(p => p.Department)
                .Include(p => p.Sport)
                .ToListAsync();

            sb.AppendLine("Team Captains and Key Players:");
            var captains = players.Where(p => p.IsCaptain).OrderBy(p => p.Department.DeptName).ThenBy(p => p.Sport.SportName);
            
            foreach (var c in captains)
            {
                sb.AppendLine($"- {c.Department.DeptName} ({c.Sport.SportName}) Captain: {c.FullName} (Reg: {c.RegNumber ?? "N/A"})");
            }
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
