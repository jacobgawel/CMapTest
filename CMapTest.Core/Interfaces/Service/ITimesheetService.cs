using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Timesheet;
using FluentResults;

namespace CMapTest.Core.Interfaces.Service;

public interface ITimesheetService
{
    Task<Result<PaginatedResponse<TimesheetResponse>>> GetAllAsync(int page, int pageSize);
    Task<Result<TimesheetResponse>> GetByIdAsync(int id);
    Task<Result<PaginatedResponse<TimesheetResponse>>> GetFilteredAsync(int? userId, int? projectId, DateOnly? startDate, DateOnly? endDate, int page, int pageSize);
    Task<Result<List<ProjectHoursSummary>>> GetProjectHoursSummaryAsync(int? userId, int? projectId, DateOnly? startDate, DateOnly? endDate);
    Task<Result<(DateOnly? MinDate, DateOnly? MaxDate)>> GetDateRangeAsync();
    Task<Result<TimesheetResponse>> CreateAsync(CreateTimesheetRequest request);
    Task<Result<TimesheetResponse>> UpdateAsync(int id, UpdateTimesheetRequest request);
    Task<Result> DeleteAsync(int id);
}
