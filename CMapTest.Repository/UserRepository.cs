using System.Diagnostics.CodeAnalysis;
using CMapTest.Core.Entities;
using CMapTest.Core.Exceptions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace CMapTest.Repository;

[ExcludeFromCodeCoverage]
public class UserRepository(CMapTestContext context) : IUserRepository
{
    public async Task<(List<UserEntity> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Users.CountAsync();
        var items = await context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
    {
        try
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to save user", ex);
        }
    }

    public async Task<UserEntity?> UpdateAsync(int id, UserEntity user)
    {
        var existing = await context.Users.FindAsync(id);
        if (existing is null) return null;

        try
        {
            existing.Name = user.Name;
            existing.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return existing;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to save user", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.Users.FindAsync(id);
        if (existing is null) return false;

        try
        {
            context.Users.Remove(existing);
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("Failed to delete user", ex);
        }
    }
}
