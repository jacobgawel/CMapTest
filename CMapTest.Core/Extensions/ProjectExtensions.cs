using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Entities;

namespace CMapTest.Core.Extensions;

public static class ProjectExtensions
{
    extension(ProjectEntity entity)
    {
        public ProjectResponse ToResponse()
            => new()
            {
                Id = entity.Id,
                Name = entity.Name
            };
    }
}
