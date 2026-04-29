using Microsoft.EntityFrameworkCore;
using SportsPortal.Models;

namespace SportsPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Sport> Sports { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; } // Changed from Schedules
        // DbSet<Result> Results is removed
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; } // Added
        public DbSet<Season> Seasons { get; set; }     // Added
        public DbSet<Team> Teams { get; set; }         // Added
        public DbSet<GalleryImage> GalleryImages { get; set; } // Added

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship for Match's departments to avoid cascade delete issues.
            modelBuilder.Entity<Match>()
                .HasOne(m => m.DepartmentA)
                .WithMany()
                .HasForeignKey(m => m.DeptA_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.DepartmentB)
                .WithMany()
                .HasForeignKey(m => m.DeptB_ID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
