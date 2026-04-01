using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.User;
using CMapTest.Core.Entities;
using CMapTest.Core.Errors;
using CMapTest.Core.Extensions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Core.Interfaces.Service;
using FluentResults;
using CMapTest.Core.Exceptions;

namespace CMapTest.Services;

public class UserService(IUserRepository userRepository, ITimesheetRepository timesheetRepository) : IUserService
{
    public async Task<Result<PaginatedResponse<UserResponse>>> GetAllAsync(int page, int pageSize)
    {
        var (items, totalCount) = await userRepository.GetAllAsync(page, pageSize);
        return Result.Ok(new PaginatedResponse<UserResponse>
        {
            Items = items.Select(u => u.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
            return Result.Fail<UserResponse>(new NotFoundError($"User with id {id} not found"));

        return Result.Ok(user.ToResponse());
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        try
        {
            var entity = new UserEntity { Name = request.Name };
            var created = await userRepository.CreateAsync(entity);
            return Result.Ok(created.ToResponse());
        }
        catch (PersistenceException)
        {
            return Result.Fail<UserResponse>("Failed to save user");
        }
    }

    public async Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request)
    {
        try
        {
            var entity = new UserEntity { Name = request.Name };
            var updated = await userRepository.UpdateAsync(id, entity);

            if (updated is null)
                return Result.Fail<UserResponse>(new NotFoundError($"User with id {id} not found"));

            return Result.Ok(updated.ToResponse());
        }
        catch (PersistenceException)
        {
            return Result.Fail<UserResponse>("Failed to save user");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var hasTimesheets = await timesheetRepository.ExistsByUserIdAsync(id);

        if (hasTimesheets)
            return Result.Fail(new ConflictError("Cannot delete user because they have associated timesheets"));

        try
        {
            var deleted = await userRepository.DeleteAsync(id);
            if (!deleted)
                return Result.Fail(new NotFoundError($"User with id {id} not found"));

            return Result.Ok();
        }
        catch (PersistenceException)
        {
            return Result.Fail(new ConflictError("Cannot delete user because they have associated timesheets"));
        }
    }
}
