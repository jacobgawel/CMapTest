using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.User;
using FluentResults;

namespace CMapTest.Core.Interfaces.Service;

public interface IUserService
{
    Task<Result<PaginatedResponse<UserResponse>>> GetAllAsync(int page, int pageSize);
    Task<Result<UserResponse>> GetByIdAsync(int id);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request);
    Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request);
    Task<Result> DeleteAsync(int id);
}
