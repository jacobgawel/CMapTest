using System.Diagnostics.CodeAnalysis;
using CMapTest.Core.Entities;
using CMapTest.Core.Exceptions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace CMapTest.Repository;

[ExcludeFromCodeCoverage]
public class ProjectRepository(CMapTestContext context) : IProjectRepository
{
    public async Task<(List<ProjectEntity> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Projects.CountAsync();
        var items = await context.Projects
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<ProjectEntity?> GetByIdAsync(int id)
    {
        return await context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProjectEntity> CreateAsync(ProjectEntity project)
    {
        try
        {
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;
            context.Projects.Add(project);
            await context.SaveChangesAsync();
            return project;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to save project", ex);
        }
    }

    public async Task<ProjectEntity?> UpdateAsync(int id, ProjectEntity project)
    {
        var existing = await context.Projects.FindAsync(id);
        if (existing is null) return null;

        try
        {
            existing.Name = project.Name;
            existing.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return existing;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to save project", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Projects.FindAsync(id);
        if (existing is null) return false;

        try
        {
            context.Projects.Remove(existing);
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to delete project", ex);
        }
    }
}