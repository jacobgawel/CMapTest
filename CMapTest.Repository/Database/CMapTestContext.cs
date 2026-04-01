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
}
