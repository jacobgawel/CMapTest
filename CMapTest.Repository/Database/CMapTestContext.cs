using CMapTest.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMapTest.Repository.Database;

public class CMapTestContext(DbContextOptions<CMapTestContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<TimesheetEntity> Timesheets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CMapTestContext).Assembly);
    }

    public void Seed()
    {
        if (Users.Any())
            return;

        var now = DateTime.UtcNow;

        var users = new[]
        {
            new UserEntity { Name = "Alice Johnson", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Bob Smith", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Charlie Davis", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Diana Martinez", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Ethan Wilson", CreatedAt = now, UpdatedAt = now },
        };
        Users.AddRange(users);
        SaveChanges();

        var projects = new[]
        {
            new ProjectEntity { Name = "Website Redesign", CreatedAt = now, UpdatedAt = now },
            new ProjectEntity { Name = "Mobile App", CreatedAt = now, UpdatedAt = now },
            new ProjectEntity { Name = "API Integration", CreatedAt = now, UpdatedAt = now },
        };
        Projects.AddRange(projects);
        SaveChanges();

        var rng = new Random(42);
        var descriptions = new[]
        {
            "Bug fix for login flow", "Set up CI pipeline", "Write unit tests",
            "Code review session", "Refactor data layer", "Design sprint planning",
            "Implement search feature", "Update dependencies", "Write API docs",
            "Performance profiling", "Database migration", "Standup and sync",
            "Spike on caching strategy", "Fix flaky integration test", "Deploy to staging",
            "Customer feedback triage", "Accessibility audit", "Schema design review",
        };
        var timesheets = new List<TimesheetEntity>();

        foreach (var user in users)
        {
            for (var i = 0; i < 10; i++)
            {
                var daysAgo = rng.Next(1, 30);
                var date = DateOnly.FromDateTime(DateTime.Today.AddDays(-daysAgo));
                var project = projects[rng.Next(projects.Length)];
                var startHour = rng.Next(7, 14);
                var durationHours = rng.Next(1, 5);
                var startMinute = rng.Next(0, 4) * 15;
                var endMinute = rng.Next(0, 4) * 15;
                timesheets.Add(new TimesheetEntity
                {
                    UserId = user.Id,
                    ProjectId = project.Id,
                    Date = date,
                    Description = descriptions[rng.Next(descriptions.Length)],
                    StartTime = new TimeOnly(startHour, startMinute),
                    EndTime = new TimeOnly(startHour + durationHours, endMinute),
                });
            }
        }

        Timesheets.AddRange(timesheets);
        SaveChanges();
    }
}
