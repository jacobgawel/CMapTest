using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Project;
using FluentResults;

namespace CMapTest.Core.Interfaces.Service;

public interface IProjectService
{
    Task<Result<PaginatedResponse<ProjectResponse>>> GetAllAsync(int page, int pageSize);
    Task<Result<ProjectResponse>> GetByIdAsync(int id);
    Task<Result<ProjectResponse>> CreateAsync(CreateProjectRequest request);
    Task<Result<ProjectResponse>> UpdateAsync(int id, UpdateProjectRequest request);
    Task<Result> DeleteAsync(int id);
}
