using CMapTest.Core.Entities;

namespace CMapTest.Core.Interfaces.Repository;

public interface ITimesheetRepository
{
    Task<(List<TimesheetEntity> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<TimesheetEntity?> GetByIdAsync(int id);
    Task<(List<TimesheetEntity> Items, int TotalCount)> GetFilteredAsync(int? userId, int? projectId, DateOnly? startDate, DateOnly? endDate, int page, int pageSize);
    Task<List<(int ProjectId, string ProjectName, decimal TotalHours)>> GetProjectHoursSummaryAsync(int? userId, int? projectId, DateOnly? startDate, DateOnly? endDate);
    Task<(DateOnly? MinDate, DateOnly? MaxDate)> GetDateRangeAsync();
    Task<bool> ExistsByUserIdAsync(int userId);
    Task<bool> ExistsByProjectIdAsync(int projectId);
    Task<bool> HasOverlappingEntryAsync(int userId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeId = null);
    Task<TimesheetEntity> CreateAsync(TimesheetEntity timesheet);
    Task<TimesheetEntity?> UpdateAsync(int id, TimesheetEntity timesheet);
    Task<bool> DeleteAsync(int id);
}
