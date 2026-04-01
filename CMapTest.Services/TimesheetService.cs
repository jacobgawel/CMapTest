using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Entities;
using CMapTest.Core.Errors;
using CMapTest.Core.Extensions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Core.Interfaces.Service;
using FluentResults;
using CMapTest.Core.Exceptions;

namespace CMapTest.Services;

public class TimesheetService(
    ITimesheetRepository timesheetRepository,
    IUserRepository userRepository,
    IProjectRepository projectRepository,
    TimeProvider timeProvider) : ITimesheetService
{
    public async Task<Result<PaginatedResponse<TimesheetResponse>>> GetAllAsync(int page, int pageSize)
    {
        var (items, totalCount) = await timesheetRepository.GetAllAsync(page, pageSize);
        return Result.Ok(new PaginatedResponse<TimesheetResponse>
        {
            Items = items.Select(e => e.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<TimesheetResponse>> GetByIdAsync(int id)
    {
        var timesheet = await timesheetRepository.GetByIdAsync(id);

        if (timesheet is null)
            return Result.Fail<TimesheetResponse>(new NotFoundError($"Timesheet with id {id} not found"));

        return Result.Ok(timesheet.ToResponse());
    }

    public async Task<Result<PaginatedResponse<TimesheetResponse>>> GetFilteredAsync(int? userId, int? projectId,
        DateOnly? startDate, DateOnly? endDate, int page, int pageSize)
    {
        var (items, totalCount) = await timesheetRepository.GetFilteredAsync(userId, projectId, startDate, endDate, page, pageSize);
        return Result.Ok(new PaginatedResponse<TimesheetResponse>
        {
            Items = items.Select(e => e.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<List<ProjectHoursSummary>>> GetProjectHoursSummaryAsync(
        int? userId, int? projectId, DateOnly? startDate, DateOnly? endDate)
    {
        var data = await timesheetRepository.GetProjectHoursSummaryAsync(userId, projectId, startDate, endDate);
        var summaries = data.Select(d => new ProjectHoursSummary
        {
            ProjectId = d.ProjectId,
            ProjectName = d.ProjectName,
            TotalHours = Math.Round(d.TotalHours, 2)
        }).ToList();
        return Result.Ok(summaries);
    }

    public async Task<Result<(DateOnly? MinDate, DateOnly? MaxDate)>> GetDateRangeAsync()
    {
        var range = await timesheetRepository.GetDateRangeAsync();
        return Result.Ok(range);
    }

    public async Task<Result<TimesheetResponse>> CreateAsync(CreateTimesheetRequest request)
    {
        var validation = await ValidateTimesheetAsync(
            request.UserId,
            request.ProjectId,
            request.Date,
            request.StartTime,
            request.EndTime);

        if (validation.IsFailed) return validation;

        var entity = new TimesheetEntity
        {
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            Date = request.Date,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        try
        {
            var created = await timesheetRepository.CreateAsync(entity);
            return Result.Ok(created.ToResponse());
        }
        catch (PersistenceException)
        {
            return Result.Fail<TimesheetResponse>("Failed to save timesheet");
        }
    }

    public async Task<Result<TimesheetResponse>> UpdateAsync(int id, UpdateTimesheetRequest request)
    {
        var validation = await ValidateTimesheetAsync(
            request.UserId,
            request.ProjectId,
            request.Date,
            request.StartTime,
            request.EndTime,
            id);

        if (validation.IsFailed) return validation;

        var entity = new TimesheetEntity
        {
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            Date = request.Date,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        try
        {
            var updated = await timesheetRepository.UpdateAsync(id, entity);

            if (updated is null)
                return Result.Fail<TimesheetResponse>(new NotFoundError($"Timesheet with id {id} not found"));

            return Result.Ok(updated.ToResponse());
        }
        catch (PersistenceException)
        {
            return Result.Fail<TimesheetResponse>("Failed to save timesheet");
        }
    }

    private async Task<Result<TimesheetResponse>> ValidateTimesheetAsync(
        int userId, int projectId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeId = null)
    {
        if (endTime <= startTime)
            return Result.Fail<TimesheetResponse>(new ValidationError("End time must be after start time"));

        if (date > DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            return Result.Fail<TimesheetResponse>(new ValidationError("Date cannot be in the future"));

        var user = await userRepository.GetByIdAsync(userId);

        if (user is null)
            return Result.Fail<TimesheetResponse>(new ValidationError($"User with id {userId} not found"));

        var project = await projectRepository.GetByIdAsync(projectId);

        if (project is null)
            return Result.Fail<TimesheetResponse>(new ValidationError($"Project with id {projectId} not found"));

        var hasOverlap = await timesheetRepository.HasOverlappingEntryAsync(userId, date, startTime, endTime, excludeId);

        if (hasOverlap)
            return Result.Fail<TimesheetResponse>(new ConflictError("This timesheet overlaps with an existing entry for this user"));

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var deleted = await timesheetRepository.DeleteAsync(id);

            if (!deleted)
                return Result.Fail(new NotFoundError($"Timesheet with id {id} not found"));

            return Result.Ok();
        }
        catch (PersistenceException)
        {
            return Result.Fail("Failed to delete timesheet");
        }
    }
}