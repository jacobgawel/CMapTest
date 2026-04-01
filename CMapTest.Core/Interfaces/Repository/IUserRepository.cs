using CMapTest.Core.Entities;

namespace CMapTest.Core.Interfaces.Repository;

public interface IUserRepository
{
    Task<(List<UserEntity> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<UserEntity?> GetByIdAsync(int id);
    Task<UserEntity> CreateAsync(UserEntity user);
    Task<UserEntity?> UpdateAsync(int id, UserEntity user);
    Task<bool> DeleteAsync(int id);
}
