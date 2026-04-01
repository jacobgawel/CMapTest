using CMapTest.Core.DTOs.User;
using CMapTest.Core.Entities;

namespace CMapTest.Core.Extensions;

public static class UserExtensions
{
    extension(UserEntity entity)
    {
        public UserResponse ToResponse()
            => new()
            {
                Id = entity.Id,
                Name = entity.Name
            };
    }
}
