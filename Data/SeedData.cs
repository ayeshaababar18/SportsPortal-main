using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SportsPortal.Models;

namespace SportsPortal.Data
{
    public static class SeedData
    {
        private static int _currentYear = DateTime.Now.Year;
        private static readonly Random _random = new Random();

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>()))
            {
                Console.WriteLine("Starting database seeding...");

                // Manual Schema Fix for missing TeamID (Database Drift Repair)
                try 
                {
                    await context.Database.ExecuteSqlRawAsync(@"
                        IF COL_LENGTH('tbl_Players', 'TeamID') IS NULL
                        BEGIN
                            PRINT 'Manually adding missing TeamID column...';
                            ALTER TABLE tbl_Players ADD TeamID int NULL;
                            CREATE INDEX IX_tbl_Players_TeamID ON tbl_Players(TeamID);
                            ALTER TABLE tbl_Players ADD CONSTRAINT FK_tbl_Players_tbl_Teams_TeamID FOREIGN KEY (TeamID) REFERENCES tbl_Teams(TeamID);
                        END
                    ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Schema repair warning: {ex.Message}");
                    // Proceeding anyway as it might be a transient issue or different DB provider
                }

                // Fix existing data categories based on sport names
                Console.WriteLine("Repairing existing data categories...");
                await FixExistingDataCategories(context);

                // Force update names
                if (context.Players.Any())
                {
                     Console.WriteLine("Forcing update of player names to Pakistani names...");
                     await UpdateExistingNamesToPakistani(context);
                }

                // Ensure Teams exist (Fix for missing teams)
                Console.WriteLine("Ensuring Teams exist...");
                await EnsureTeamsExist(context);

                var alreadySeeded = context.Seasons.Any() || context.AdminUsers.Any();
                if (alreadySeeded)
                {
                    Console.WriteLine("Database has already been seeded. Running supplemental student seeding.");
                    Console.WriteLine("Ensuring all student players exist in every team...");
                    await EnsurePlayersExist(context);
                    Console.WriteLine("Updating existing names to Pakistani names...");
                    await UpdateExistingNamesToPakistani(context);
                    Console.WriteLine("Calculating and saving total points...");
                    await CalculateAndSaveTotalPoints(context);
                    Console.WriteLine("Total points calculated and saved.");
                    Console.WriteLine("Database seeding finished.");
                    return;
                }

                Console.WriteLine("Seeding Admin Users...");
                if (!context.AdminUsers.Any())
                {
                    context.AdminUsers.Add(new AdminUser
                    {
                        Username = "ayesha",
                        Password = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2c6dd68f4d",
                        Role = "Organizer"
                    });
                    context.AdminUsers.Add(new AdminUser
                    {
                        Username = "mujtaba",
                        Password = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2c6dd68f4d",
                        Role = "Organizer"
                    });
                    await context.SaveChangesAsync();
                    Console.WriteLine("Admin Users seeded.");
                }

                Console.WriteLine("Seeding default season...");
                var defaultSeason = new Season { Year = _currentYear, IsActive = true };
                context.Seasons.Add(defaultSeason);
                await context.SaveChangesAsync();
                Console.WriteLine("Default season seeded.");

                var currentSeason = await context.Seasons.FirstOrDefaultAsync(s => s.IsActive);
                if (currentSeason == null)
                {
                    currentSeason = defaultSeason;
                }

                Console.WriteLine("Seeding departments...");
                var departments = new List<Department>
                {
                    new Department { DeptName = "Computer Science", LogoUrl = "/images/dept_logos/cs.png" },
                    new Department { DeptName = "Management Sciences", LogoUrl = "/images/dept_logos/mgmt.png" },
                    new Department { DeptName = "Software Engineering", LogoUrl = "/images/dept_logos/se.png" },
                    new Department { DeptName = "Electrical & Computer Engineering", LogoUrl = "/images/dept_logos/ece.png" },
                    new Department { DeptName = "Mathematics", LogoUrl = "/images/dept_logos/math.png" },
                    new Department { DeptName = "Accounting & Finance", LogoUrl = "/images/dept_logos/accfin.png" },
                    new Department { DeptName = "Law", LogoUrl = "/images/dept_logos/law.png" },
                    new Department { DeptName = "Mechanical Engineering", LogoUrl = "/images/dept_logos/mech.png" },
                    new Department { DeptName = "Bio Sciences", LogoUrl = "/images/dept_logos/bio.png" },
                    new Department { DeptName = "English", LogoUrl = "/images/dept_logos/eng.png" },
                    new Department { DeptName = "Associate Degree Program", LogoUrl = "/images/dept_logos/adp.png" },
                    new Department { DeptName = "Civil Engineering", LogoUrl = "/images/dept_logos/civil.png" },
                    new Department { DeptName = "Psychology", LogoUrl = "/images/dept_logos/psy.png" },
                    new Department { DeptName = "Artificial Intelligence", LogoUrl = "/images/dept_logos/ai.png" },
                    new Department { DeptName = "Pharmacy", LogoUrl = "/images/dept_logos/pharm.png" }
                };
                foreach (var dept in departments)
                {
                    if (!context.Departments.Any(d => d.DeptName == dept.DeptName))
                    {
                        context.Departments.Add(dept);
                    }
                }
                await context.SaveChangesAsync();
                Console.WriteLine("Departments seeded.");

                var seededDepartments = await context.Departments.ToListAsync();

                Console.WriteLine("Seeding sports...");
                var seededSports = new List<Sport>
                {
                    new Sport { SportName = "Girls Cricket" },
                    new Sport { SportName = "Boys Athletics" },
                    new Sport { SportName = "Football" },
                    new Sport { SportName = "Basketball" },
                    new Sport { SportName = "Volleyball" },
                    new Sport { SportName = "Table Tennis" },
                    new Sport { SportName = "Tug of War" },
                    new Sport { SportName = "Squash" },
                    new Sport { SportName = "Ludo" },
                    new Sport { SportName = "Chess" }
                };
                foreach (var sport in seededSports)
                {
                    if (!context.Sports.Any(s => s.SportName == sport.SportName))
                    {
                        context.Sports.Add(sport);
                    }
                }
                await context.SaveChangesAsync();
                Console.WriteLine("Sports seeded.");

                var sportsInDb = await context.Sports.ToListAsync();

                Console.WriteLine("Processing CSVs and generating generic sports data...");
                await ProcessGirlsCricketCsv("Draws Girls-SW25-1(Tentative).csv", context, seededDepartments, sportsInDb.FirstOrDefault(s => s.SportName == "Girls Cricket")!, currentSeason!);
                await ProcessBoysAthleticsCsv("Draws Boys-SW25-1(Tentative).csv", context, seededDepartments, sportsInDb.FirstOrDefault(s => s.SportName == "Boys Athletics")!, currentSeason!);

                foreach (var department in seededDepartments.Where(d => d.DeptName != "All Departments"))
                {
                    foreach (var sport in sportsInDb)
                    {
                        await ProcessGenericSport(department, sport, context, currentSeason!);
                    }
                }
                Console.WriteLine("CSVs and generic sports data processed.");

                Console.WriteLine("Ensuring Teams exist...");
                await EnsureTeamsExist(context);
                Console.WriteLine("Teams verified.");

                Console.WriteLine("Calculating and saving total points...");
                await CalculateAndSaveTotalPoints(context);
                Console.WriteLine("Total points calculated and saved.");

                // Temporary fix to update existing data
                Console.WriteLine("Updating existing names to Pakistani names...");
                await UpdateExistingNamesToPakistani(context);
                Console.WriteLine("Names updated.");

                Console.WriteLine("Database seeding finished.");
            }
        }

        private static async Task UpdateExistingNamesToPakistani(ApplicationDbContext context)
        {
            var players = await context.Players.ToListAsync();
            foreach (var player in players)
            {
                player.FullName = GenerateRandomName();
            }
            context.UpdateRange(players);
            await context.SaveChangesAsync();
        }

        private static async Task ProcessGirlsCricketCsv(string filePath, ApplicationDbContext context, List<Department> departments, Sport sport, Season season)
        {
            if (!File.Exists(filePath)) return;
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length < 2) return;

            var matches = new List<Match>();
            for (int i = 2; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("Match")) continue;

                var parts = line.Split(',');
                if (parts.Length < 6) continue;

                var deptA_Name = parts[3].Trim();
                var deptB_Name = parts[4].Trim();

                if (DateTime.TryParseExact(parts[1].Trim(), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                    !string.IsNullOrEmpty(parts[2].Trim()))
                {
                    var timeParts = parts[2].Trim().Split('-')[0].Trim().Split(':');
                    if (timeParts.Length == 2 && int.TryParse(timeParts[0], out var hour) && int.TryParse(timeParts[1], out var minute))
                    {
                        var matchDateTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);

                        if (deptA_Name != "BYE" && !deptA_Name.StartsWith("W") && deptB_Name != "BYE" && !deptB_Name.StartsWith("W"))
                        {
                            var departmentA = departments.FirstOrDefault(d => d.DeptName == deptA_Name);
                            var departmentB = departments.FirstOrDefault(d => d.DeptName == deptB_Name);

                            if (departmentA != null && departmentB != null)
                            {
                                var matchStatus = "Scheduled";
                                if (matchDateTime < DateTime.Now.AddDays(-2)) matchStatus = "Finished";
                                else if (matchDateTime < DateTime.Now.AddHours(2) && matchDateTime > DateTime.Now.AddHours(-2)) matchStatus = "Live";

                                matches.Add(new Match
                                {
                                    Sport = sport,
                                    DepartmentA = departmentA,
                                    DepartmentB = departmentB,
                                    MatchDate = matchDateTime,
                                    Status = matchStatus,
                                    Season = season,
                                    Category = TeamCategory.Girls,
                                    ScoreA = matchStatus == "Finished" ? $"{_random.Next(100, 200)}/{_random.Next(0, 10)}" : null,
                                    ScoreB = matchStatus == "Finished" ? $"{_random.Next(100, 200)}/{_random.Next(0, 10)}" : null
                                });
                            }
                        }
                    }
                }
            }
            context.Matches.AddRange(matches);
            await context.SaveChangesAsync();
        }

        private static async Task ProcessBoysAthleticsCsv(string filePath, ApplicationDbContext context, List<Department> departments, Sport sport, Season season)
        {
            if (!File.Exists(filePath)) return;
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length < 4) return;

            var matches = new List<Match>();
            for (int i = 4; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || !char.IsDigit(line[0])) continue;

                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                if (DateTime.TryParseExact(parts[0].Trim(), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                    !string.IsNullOrEmpty(parts[2].Trim()))
                {
                    var timeParts = parts[2].Trim().Split('-')[0].Trim().Split(':');
                    if (timeParts.Length == 2 && int.TryParse(timeParts[0], out var hour) && int.TryParse(timeParts[1], out var minute))
                    {
                        var matchDateTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);

                        var matchStatus = "Scheduled";
                        if (matchDateTime < DateTime.Now.AddDays(-2)) matchStatus = "Finished";
                        else if (matchDateTime < DateTime.Now.AddHours(2) && matchDateTime > DateTime.Now.AddHours(-2)) matchStatus = "Live";

                        matches.Add(new Match
                        {
                            Sport = sport,
                            MatchDate = matchDateTime,
                            Status = matchStatus,
                            Season = season,
                            Category = TeamCategory.Boys
                        });
                    }
                }
            }
            context.Matches.AddRange(matches);
            await context.SaveChangesAsync();
        }

        private static async Task ProcessGenericSport(Department department, Sport sport, ApplicationDbContext context, Season season)
        {
            // Determine if the sport is gender-specific or generic
            bool isExplicitlyGirls = sport.SportName.Contains("Girls", StringComparison.OrdinalIgnoreCase);
            bool isExplicitlyBoys = sport.SportName.Contains("Boys", StringComparison.OrdinalIgnoreCase);
            bool isGeneric = !isExplicitlyGirls && !isExplicitlyBoys;

            // Seed Players - We seed both genders if it's a generic sport
            var gendersToSeed = new List<Gender>();
            if (isExplicitlyGirls) gendersToSeed.Add(Gender.Female);
            else if (isExplicitlyBoys) gendersToSeed.Add(Gender.Male);
            else { gendersToSeed.Add(Gender.Male); gendersToSeed.Add(Gender.Female); }

            foreach (var gender in gendersToSeed)
            {
                var category = gender == Gender.Male ? TeamCategory.Boys : TeamCategory.Girls;
                var team = await context.Teams.FirstOrDefaultAsync(t => t.DeptID == department.DeptID && t.SportID == sport.SportID && t.Category == category);

                if (team == null)
                {
                    team = new Team
                    {
                        TeamName = $"{department.DeptName} {sport.SportName} ({category})",
                        DeptID = department.DeptID,
                        SportID = sport.SportID,
                        Category = category
                    };
                    context.Teams.Add(team);
                    await context.SaveChangesAsync();
                }

                if (!context.Players.Any(p => p.TeamID == team.TeamID))
                {
                    // Add a captain
                    context.Players.Add(new Player
                    {
                        FullName = GenerateRandomName(gender),
                        RegNumber = Guid.NewGuid().ToString().Substring(0, 8),
                        DeptID = department.DeptID,
                        SportID = sport.SportID,
                        TeamID = team.TeamID,
                        IsCaptain = true,
                        Gender = gender
                    });

                    // Add other players
                    int numberOfPlayers = _random.Next(5, 8);
                    for (int i = 0; i < numberOfPlayers; i++)
                    {
                        context.Players.Add(new Player
                        {
                            FullName = GenerateRandomName(gender),
                            RegNumber = Guid.NewGuid().ToString().Substring(0, 8),
                            DeptID = department.DeptID,
                            SportID = sport.SportID,
                            TeamID = team.TeamID,
                            IsCaptain = false,
                            Gender = gender
                        });
                    }
                }
            }

            var otherDepartments = await context.Departments.Where(d => d.DeptID != department.DeptID && d.DeptName != "All Departments").ToListAsync();

            if (otherDepartments.Any())
            {
                // Create matches between departments
                foreach (var otherDept in otherDepartments.Take(2))
                {
                    var categoriesToCreate = new List<TeamCategory>();
                    if (isExplicitlyGirls) categoriesToCreate.Add(TeamCategory.Girls);
                    else if (isExplicitlyBoys) categoriesToCreate.Add(TeamCategory.Boys);
                    else { categoriesToCreate.Add(TeamCategory.Boys); categoriesToCreate.Add(TeamCategory.Girls); }

                    foreach (var category in categoriesToCreate)
                    {
                        var matchTime = DateTime.Now.AddDays(_random.Next(-15, 5)).AddHours(_random.Next(9, 18)).AddMinutes(_random.Next(0, 59));
                        var matchStatus = "Scheduled";
                        if (matchTime < DateTime.Now) matchStatus = "Finished";
                        else if (matchTime < DateTime.Now.AddHours(2) && matchTime > DateTime.Now.AddHours(-2)) matchStatus = "Live";

                        if (!context.Matches.Any(m => m.SportID == sport.SportID &&
                                                    m.Category == category &&
                                                    ((m.DeptA_ID == department.DeptID && m.DeptB_ID == otherDept.DeptID) ||
                                                     (m.DeptA_ID == otherDept.DeptID && m.DeptB_ID == department.DeptID)) &&
                                                     Math.Abs((m.MatchDate - matchTime).TotalMinutes) < 60))
                        {
                            context.Matches.Add(new Match
                            {
                                Sport = sport,
                                DepartmentA = department,
                                DepartmentB = otherDept,
                                MatchDate = matchTime,
                                Status = matchStatus,
                                Season = season,
                                Category = category,
                                ScoreA = matchStatus == "Finished" ? $"{_random.Next(0, 50)}" : null,
                                ScoreB = matchStatus == "Finished" ? $"{_random.Next(0, 50)}" : null
                            });
                        }
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task EnsureTeamsExist(ApplicationDbContext context)
        {
            var departments = await context.Departments.ToListAsync();
            var sports = await context.Sports.ToListAsync();

            foreach (var dept in departments)
            {
                if (dept.DeptName == "All Departments") continue;

                foreach (var sport in sports)
                {
                    var categories = new List<TeamCategory>();
                    if (sport.SportName.Contains("Girls", StringComparison.OrdinalIgnoreCase)) categories.Add(TeamCategory.Girls);
                    else if (sport.SportName.Contains("Boys", StringComparison.OrdinalIgnoreCase)) categories.Add(TeamCategory.Boys);
                    else { categories.Add(TeamCategory.Boys); categories.Add(TeamCategory.Girls); }

                    foreach (var cat in categories)
                    {
                        if (!context.Teams.Any(t => t.DeptID == dept.DeptID && t.SportID == sport.SportID && t.Category == cat))
                        {
                            context.Teams.Add(new Team
                            {
                                TeamName = $"{dept.DeptName} {sport.SportName} ({cat})",
                                DeptID = dept.DeptID,
                                SportID = sport.SportID,
                                Category = cat
                            });
                        }
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task EnsurePlayersExist(ApplicationDbContext context)
        {
            var teams = await context.Teams.ToListAsync();
            foreach (var team in teams)
            {
                if (context.Players.Any(p => p.TeamID == team.TeamID))
                {
                    continue;
                }

                var gender = team.Category == TeamCategory.Girls ? Gender.Female : Gender.Male;
                var captain = new Player
                {
                    FullName = GenerateRandomName(gender),
                    RegNumber = GenerateRegistrationNumber(team),
                    DeptID = team.DeptID,
                    SportID = team.SportID,
                    TeamID = team.TeamID,
                    IsCaptain = true,
                    Gender = gender
                };
                context.Players.Add(captain);

                int numberOfPlayers = 8;
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    context.Players.Add(new Player
                    {
                        FullName = GenerateRandomName(gender),
                        RegNumber = GenerateRegistrationNumber(team),
                        DeptID = team.DeptID,
                        SportID = team.SportID,
                        TeamID = team.TeamID,
                        IsCaptain = false,
                        Gender = gender
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static string GenerateRegistrationNumber(Team team)
        {
            var deptPrefix = string.Concat(team.DeptID.ToString().PadLeft(2, '0'));
            var sportPrefix = string.Concat(team.SportID.ToString().PadLeft(2, '0'));
            var yearSuffix = DateTime.Now.Year.ToString().Substring(2);
            return $"{deptPrefix}-{sportPrefix}-{yearSuffix}-{_random.Next(1000, 9999)}";
        }

        private static async Task FixExistingDataCategories(ApplicationDbContext context)
        {
            // Fix Matches
            var matches = await context.Matches.Include(m => m.Sport).ToListAsync();
            foreach (var match in matches)
            {
                if (match.Sport == null) continue;
                if (match.Sport.SportName.Contains("Girls", StringComparison.OrdinalIgnoreCase))
                {
                    match.Category = TeamCategory.Girls;
                }
                else if (match.Sport.SportName.Contains("Boys", StringComparison.OrdinalIgnoreCase))
                {
                    match.Category = TeamCategory.Boys;
                }
            }

            // Fix Teams
            var teams = await context.Teams.Include(t => t.Sport).ToListAsync();
            foreach (var team in teams)
            {
                if (team.Sport == null) continue;
                if (team.Sport.SportName.Contains("Girls", StringComparison.OrdinalIgnoreCase))
                {
                    team.Category = TeamCategory.Girls;
                }
                else if (team.Sport.SportName.Contains("Boys", StringComparison.OrdinalIgnoreCase))
                {
                    team.Category = TeamCategory.Boys;
                }
            }

            await context.SaveChangesAsync();
        }

        private static string GenerateRandomName(Gender? gender = null)
        {
            string[] maleFirstNames = { 
                "Abbas", "Abdul", "Abdullah", "Adeel", "Adnan", "Affan", "Aftab", "Ahmad", "Ahmed", "Ahsan", 
                "Ajmal", "Akbar", "Akmal", "Alam", "Aleem", "Ali", "Altaf", "Aman", "Amir", "Amjad", 
                "Ammar", "Anis", "Anwar", "Aqeel", "Arham", "Arif", "Arshad", "Asad", "Asghar", "Ashraf", 
                "Asif", "Asim", "Aslam", "Athar", "Atif", "Ayaz", "Azhar", "Aziz", "Babar", "Basit", 
                "Bilal", "Danish", "Danyal", "Dawood", "Dilawar", "Ehsan", "Fahad", "Faizan", "Faraz", "Fareed", 
                "Farhan", "Farid", "Farooq", "Fawad", "Faysal", "Feroz", "Ghaffar", "Ghafoor", "Ghulam", "Habib", 
                "Hadi", "Hafeez", "Hamid", "Hammad", "Hamza", "Haris", "Haroon", "Haseeb", "Hashim", "Hassan", 
                "Hussain", "Huzaifa", "Ibrahim", "Idrees", "Iftikhar", "Ilyas", "Imran", "Inam", "Iqbal", "Irfan", 
                "Ishtiaq", "Ismail", "Jameel", "Jamshed", "Javed", "Junaid", "Kamran", "Kashif", "Khalid", "Khurram", 
                "Luqman", "Mahmood", "Majid", "Mansoor", "Maqsood", "Masood", "Mateen", "Mazhar", "Mehmood", "Mohsin", 
                "Mubashir", "Mudassir", "Muhammad", "Mujtaba", "Muneeb", "Mushtaq", "Mustafa", "Muzammil", "Nabeel", "Nadeem", 
                "Naeem", "Noman", "Nasir", "Naveed", "Nawaz", "Nisar", "Noor", "Omar", "Osama", "Parvez", 
                "Qasim", "Raheel", "Raheem", "Rahman", "Rashid", "Rauf", "Raza", "Rehan", "Rizwan", "Saad", 
                "Sabir", "Saeed", "Saif", "Sajjad", "Saleem", "Salman", "Sameer", "Saqib", "Sarfraz", "Sarmad", 
                "Shadab", "Shafiq", "Shahbaz", "Shahid", "Shahzad", "Shakeel", "Shamim", "Sheraz", "Shoaib", "Sohail", 
                "Sulaiman", "Sultan", "Tahir", "Talha", "Tanveer", "Tariq", "Tauseef", "Tufail", "Umair", "Umar", 
                "Usama", "Usman", "Waheed", "Waleed", "Waqar", "Waqas", "Waseem", "Yasir", "Younas", "Yousaf", 
                "Zafar", "Zaheer", "Zahid", "Zain", "Zakariya", "Zeeshan", "Zia", "Zubair", "Zulfiqar"
            };

            string[] femaleFirstNames = { 
                "Abida", "Adeela", "Afshan", "Aiza", "Aleena", "Alishba", "Aliya", "Amara", "Amber", "Amna", 
                "Anam", "Aneesa", "Anila", "Anum", "Aqsa", "Areeba", "Arifa", "Asma", "Asifa", "Atiqa", 
                "Ayesha", "Azra", "Beenish", "Bushra", "Dua", "Eiman", "Esha", "Faiza", "Fakhra", "Farah", 
                "Fareeha", "Farhat", "Fariha", "Farzana", "Fatima", "Fozia", "Ghazala", "Gul", "Hadiya", "Hafsa", 
                "Haleema", "Hania", "Hina", "Hira", "Huma", "Humaira", "Iffat", "Iqra", "Iram", "Javeria", 
                "Kainat", "Kashmala", "Khalida", "Khadija", "Kinza", "Kiran", "Komal", "Laiba", "Lubna", "Madiha", 
                "Maham", "Maheen", "Mahnoor", "Maira", "Maliha", "Malika", "Maria", "Maryam", "Mehnaz", "Mehwish", 
                "Mina", "Misbah", "Momal", "Momina", "Munaza", "Nadia", "Naila", "Najma", "Nasim", "Nasreen", 
                "Natasha", "Nausheen", "Nazia", "Neelam", "Nida", "Nimra", "Nisha", "Noor", "Noreen", "Nuzhat", 
                "Parveen", "Quratulain", "Rabia", "Raheela", "Rahila", "Rashida", "Razia", "Rehana", "Rida", "Rimsha", 
                "Robina", "Rubina", "Rukhsana", "Rukhsar", "Saba", "Sabiha", "Sadia", "Safia", "Saima", "Saira", 
                "Sajida", "Samina", "Samra", "Sana", "Sania", "Sara", "Seema", "Seher", "Shabana", "Shagufta", 
                "Shaheen", "Shahida", "Shazia", "Shehnaz", "Shumaila", "Sidra", "Sobia", "Sonia", "Sumaira", "Sumbul", 
                "Syeda", "Tahira", "Tania", "Tanzeela", "Tasneem", "Tayyaba", "Tehreem", "Urwa", "Uzma", "Wajiha", 
                "Warda", "Yasmin", "Yusra", "Zainab", "Zaira", "Zara", "Zareen", "Zoya", "Zunaira"
            };

            string[] lastNames = { 
                "Abbas", "Abbasi", "Abid", "Afridi", "Afzal", "Agha", "Ahmed", "Ajmal", "Akbar", "Akram", 
                "Alam", "Ali", "Alvi", "Amin", "Amir", "Ansari", "Anwar", "Arain", "Arshad", "Ashraf", 
                "Aslam", "Awan", "Ayub", "Aziz", "Baig", "Baloch", "Bashir", "Bhatti", "Bhutto", "Bukhari", 
                "Butt", "Chandio", "Chaudhry", "Cheema", "Chishti", "Chughtai", "Dad", "Dar", "Dasti", "Dawood", 
                "Din", "Dogar", "Durrani", "Ebrahim", "Elahi", "Fahad", "Farooqi", "Fazal", "Gardezi", "Ghaffar", 
                "Ghani", "Gill", "Gilani", "Gondal", "Gujjar", "Gul", "Habib", "Hadi", "Hafeez", "Hai", 
                "Haider", "Hamdani", "Hameed", "Hamid", "Haq", "Hasan", "Hashmi", "Hassan", "Hayat", "Hussain", 
                "Hyder", "Ibrahim", "Idrees", "Iftikhar", "Ilyas", "Imam", "Imran", "Inam", "Iqbal", "Irfan", 
                "Ishaq", "Ishtiaq", "Ismail", "Jabbar", "Jafri", "Jahangir", "Jalal", "Jamal", "Jamil", "Jan", 
                "Janjua", "Javed", "Jokhio", "Junaid", "Junejo", "Jutt", "Kabir", "Kakar", "Kamal", "Kamran", 
                "Karim", "Kasuri", "Kazi", "Kazmi", "Khokhar", "Khalid", "Khalil", "Khan", "Khatak", "Khawaja", 
                "Khurshid", "Kiani", "Laghari", "Lakhani", "Latif", "Lodhi", "Lone", "Magsi", "Mahmood", "Majeed", 
                "Makhdoom", "Malik", "Mangi", "Manzoor", "Maqbool", "Maqsood", "Masood", "Mazari", "Mehdi", "Mehmood", 
                "Memon", "Mengal", "Merchant", "Mian", "Mir", "Mirza", "Mohiuddin", "Mughal", "Muhammad", "Mukhtar", 
                "Mumtaz", "Munir", "Murad", "Mustafa", "Muzaffar", "Nabi", "Nadeem", "Naeem", "Naqvi", "Naseer", 
                "Nasir", "Nawaz", "Niaz", "Niazi", "Noor", "Paracha", "Pasha", "Patel", "Pathan", "Peerzada", 
                "Pervez", "Pir", "Qadir", "Qaisar", "Qasim", "Qazi", "Qureshi", "Rafiq", "Rahim", "Rahman", 
                "Rais", "Raja", "Rajput", "Ramzan", "Rana", "Rani", "Rashid", "Rasool", "Rauf", "Raza", 
                "Razzaq", "Rehman", "Riaz", "Rizvi", "Rizwan", "Sabir", "Sadiq", "Saeed", "Safdar", "Sahi", 
                "Saif", "Sajjad", "Salahuddin", "Salam", "Saleem", "Salman", "Sami", "Saqib", "Sarfraz", "Sattar", 
                "Sayed", "Sethi", "Shabir", "Shafiq", "Shah", "Shahbaz", "Shahid", "Shahzad", "Shaikh", "Shakir", 
                "Shams", "Sharif", "Sheikh", "Sher", "Sherazi", "Shoaib", "Siddiqui", "Soomro", "Sufi", "Sultan", 
                "Syed", "Tahir", "Talha", "Tareen", "Tariq", "Tufail", "Umer", "Usman", "Waheed", "Wahid", 
                "Wali", "Waraich", "Waseem", "Yaqoob", "Yasin", "Younas", "Yousaf", "Zafar", "Zaheer", "Zahid", 
                "Zaidi", "Zaman", "Zia", "Zubair", "Zulfiqar"
            };

            var selectedGender = gender ?? (_random.Next(2) == 0 ? Gender.Male : Gender.Female);

            string firstName = (selectedGender == Gender.Male) 
                ? maleFirstNames[_random.Next(maleFirstNames.Length)]
                : femaleFirstNames[_random.Next(femaleFirstNames.Length)];

            string lastName = lastNames[_random.Next(lastNames.Length)];
            return $"{firstName} {lastName}";
        }

        private static async Task CalculateAndSaveTotalPoints(ApplicationDbContext context)
        {
            var departments = await context.Departments.ToListAsync();
            foreach (var department in departments)
            {
                department.TotalPoints = 0;
                var finishedMatches = await context.Matches
                    .Where(m => m.Status == "Finished" && (m.DeptA_ID == department.DeptID || m.DeptB_ID == department.DeptID))
                    .ToListAsync();

                foreach (var match in finishedMatches)
                {
                    if (match.ScoreA != null && match.ScoreB != null)
                    {
                        var scoreA = ParseScore(match.ScoreA);
                        var scoreB = ParseScore(match.ScoreB);
                        if (match.DeptA_ID == department.DeptID)
                        {
                            if (scoreA > scoreB) department.TotalPoints += 2;
                            else if (scoreA == scoreB) department.TotalPoints += 1;
                        }
                        else if (match.DeptB_ID == department.DeptID)
                        {
                            if (scoreB > scoreA) department.TotalPoints += 2;
                            else if (scoreA == scoreB) department.TotalPoints += 1;
                        }
                    }
                }
                context.Departments.Update(department);
            }
            await context.SaveChangesAsync();
        }

        private static int ParseScore(string score)
        {
            if (string.IsNullOrEmpty(score)) return 0;
            if (score.Contains('/'))
            {
                var parts = score.Split('/');
                if (int.TryParse(parts[0], out int s)) return s;
            }
            if (int.TryParse(score, out int result)) return result;
            return 0;
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
