using CMapTest.Core.Entities;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Core.Interfaces.Service;
using CMapTest.Repository;
using CMapTest.Repository.Database;
using CMapTest.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddRazorPages();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();

builder.Services.AddDbContext<CMapTestContext>(options =>
{
    options.UseInMemoryDatabase("CMapTest");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Adding some mock data
    var db = scope.ServiceProvider.GetRequiredService<CMapTestContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var now = DateTime.UtcNow;

        var users = new[]
        {
            new UserEntity { Name = "Alice Johnson", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Bob Smith", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Charlie Davis", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Diana Martinez", CreatedAt = now, UpdatedAt = now },
            new UserEntity { Name = "Ethan Wilson", CreatedAt = now, UpdatedAt = now },
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var projects = new[]
        {
            new ProjectEntity { Name = "Website Redesign", CreatedAt = now, UpdatedAt = now },
            new ProjectEntity { Name = "Mobile App", CreatedAt = now, UpdatedAt = now },
            new ProjectEntity { Name = "API Integration", CreatedAt = now, UpdatedAt = now },
        };
        db.Projects.AddRange(projects);
        db.SaveChanges();

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

        db.Timesheets.AddRange(timesheets);
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSerilogRequestLogging();
app.MapRazorPages();

app.Run();
