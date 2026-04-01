using CMapTest.Core.Entities;

namespace CMapTest.Core.Interfaces.Repository;

public interface IProjectRepository
{
    Task<(List<ProjectEntity> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<ProjectEntity?> GetByIdAsync(int id);
    Task<ProjectEntity> CreateAsync(ProjectEntity project);
    Task<ProjectEntity?> UpdateAsync(int id, ProjectEntity project);
    Task<bool> DeleteAsync(int id);
}
