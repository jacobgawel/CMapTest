using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Entities;
using CMapTest.Core.Errors;
using CMapTest.Core.Extensions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Core.Interfaces.Service;
using FluentResults;
using CMapTest.Core.Exceptions;

namespace CMapTest.Services;

public class ProjectService(IProjectRepository projectRepository, ITimesheetRepository timesheetRepository) : IProjectService
{
    public async Task<Result<PaginatedResponse<ProjectResponse>>> GetAllAsync(int page, int pageSize)
    {
        var (items, totalCount) = await projectRepository.GetAllAsync(page, pageSize);
        return Result.Ok(new PaginatedResponse<ProjectResponse>
        {
            Items = items.Select(p => p.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<ProjectResponse>> GetByIdAsync(int id)
    {
        var project = await projectRepository.GetByIdAsync(id);
        if (project is null)
            return Result.Fail<ProjectResponse>(new NotFoundError($"Project with id {id} not found"));

        return Result.Ok(project.ToResponse());
    }

    public async Task<Result<ProjectResponse>> CreateAsync(CreateProjectRequest request)
    {
        try
        {
            var entity = new ProjectEntity { Name = request.Name };
            var created = await projectRepository.CreateAsync(entity);
            return Result.Ok(created.ToResponse());
        }
        catch (PersistenceException)
        {
            return Result.Fail<ProjectResponse>("Failed to save project");
        }
    }

    public async Task<Result<ProjectResponse>> UpdateAsync(int id, UpdateProjectRequest request)
    {
        try
        {
            var entity = new ProjectEntity { Name = request.Name };
            var updated = await projectRepository.UpdateAsync(id, entity);
            if (updated is null)
                return Result.Fail<ProjectResponse>(new NotFoundError($"Project with id {id} not found"));

            return Result.Ok(updated.ToResponse());
        }
        catch (PersistenceException)
        {
            return Result.Fail<ProjectResponse>("Failed to save project");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var hasTimesheets = await timesheetRepository.ExistsByProjectIdAsync(id);

        if (hasTimesheets)
            return Result.Fail(new ConflictError("Cannot delete project because it has associated timesheets"));

        try
        {
            var deleted = await projectRepository.DeleteAsync(id);
            if (!deleted)
                return Result.Fail(new NotFoundError($"Project with id {id} not found"));

            return Result.Ok();
        }
        catch (PersistenceException)
        {
            return Result.Fail(new ConflictError("Cannot delete project because it has associated timesheets"));
        }
    }
}
