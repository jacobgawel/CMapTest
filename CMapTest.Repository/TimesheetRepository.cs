using System.Diagnostics.CodeAnalysis;
using CMapTest.Core.Entities;
using CMapTest.Core.Exceptions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace CMapTest.Repository;

[ExcludeFromCodeCoverage]
public class TimesheetRepository(CMapTestContext context) : ITimesheetRepository
{
    public async Task<(List<TimesheetEntity> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Timesheets.CountAsync();

        var items = await context.Timesheets
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Project)
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<TimesheetEntity?> GetByIdAsync(int id)
    {
        return await context.Timesheets
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(List<TimesheetEntity> Items, int TotalCount)> GetFilteredAsync(int? userId, int? projectId,
        DateOnly? startDate, DateOnly? endDate, int page, int pageSize)
    {
        var query = context.Timesheets
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Project)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<(int ProjectId, string ProjectName, decimal TotalHours)>> GetProjectHoursSummaryAsync(
        int? userId, int? projectId, DateOnly? startDate, DateOnly? endDate)
    {
        var query = context.Timesheets
            .AsNoTracking()
            .Include(t => t.Project)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var entries = await query.ToListAsync();

        return entries
            .GroupBy(t => new { t.ProjectId, ProjectName = t.Project?.Name ?? "Unknown" })
            .Select(g => (g.Key.ProjectId, g.Key.ProjectName,
                (decimal)g.Sum(t => (t.EndTime - t.StartTime).TotalHours)))
            .OrderBy(x => x.ProjectName)
            .ToList();
    }

    public async Task<(DateOnly? MinDate, DateOnly? MaxDate)> GetDateRangeAsync()
    {
        if (!await context.Timesheets.AnyAsync())
            return (null, null);

        var minDate = await context.Timesheets.MinAsync(t => t.Date);
        var maxDate = await context.Timesheets.MaxAsync(t => t.Date);
        return (minDate, maxDate);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId)
    {
        return await context.Timesheets.AnyAsync(t => t.UserId == userId);
    }

    public async Task<bool> ExistsByProjectIdAsync(int projectId)
    {
        return await context.Timesheets.AnyAsync(t => t.ProjectId == projectId);
    }

    public async Task<bool> HasOverlappingEntryAsync(int userId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeId = null)
    {
        return await context.Timesheets
            .Where(t => t.UserId == userId && t.Date == date)
            .Where(t => excludeId == null || t.Id != excludeId)
            .Where(t => t.StartTime < endTime && t.EndTime > startTime)
            .AnyAsync();
    }

    public async Task<TimesheetEntity> CreateAsync(TimesheetEntity timesheet)
    {
        try
        {
            context.Timesheets.Add(timesheet);
            await context.SaveChangesAsync();
            return timesheet;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to save timesheet", ex);
        }
    }

    public async Task<TimesheetEntity?> UpdateAsync(int id, TimesheetEntity timesheet)
    {
        var existing = await context.Timesheets.FindAsync(id);
        if (existing is null) return null;

        try
        {
            existing.UserId = timesheet.UserId;
            existing.ProjectId = timesheet.ProjectId;
            existing.Date = timesheet.Date;
            existing.Description = timesheet.Description;
            existing.StartTime = timesheet.StartTime;
            existing.EndTime = timesheet.EndTime;
            await context.SaveChangesAsync();
            return existing;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to save timesheet", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Timesheets.FindAsync(id);
        if (existing is null) return false;

        try
        {
            context.Timesheets.Remove(existing);
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to delete timesheet", ex);
        }
    }
}