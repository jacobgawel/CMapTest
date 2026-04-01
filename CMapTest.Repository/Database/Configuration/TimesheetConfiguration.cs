using CMapTest.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMapTest.Repository.Database.Configuration;

public class TimesheetConfiguration : IEntityTypeConfiguration<TimesheetEntity>
{
    public void Configure(EntityTypeBuilder<TimesheetEntity> builder)
    {
        builder.ToTable("Timesheets");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Description).HasMaxLength(500);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ProjectId);

        builder.HasOne(e => e.Project)
            .WithMany(e => e.Timesheets)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(e => e.Timesheets)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
