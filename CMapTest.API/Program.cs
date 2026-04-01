using CMapTest.Core.Interfaces.Repository;
using CMapTest.Core.Interfaces.Service;
using CMapTest.Repository;
using CMapTest.Repository.Database;
using CMapTest.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<CMapTestContext>(options =>
{
    options.UseInMemoryDatabase("CMapTest");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CMapTestContext>();
    db.Database.EnsureCreated();
    db.Seed();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();

app.MapControllers();

app.UseExceptionHandler();

app.Run();
