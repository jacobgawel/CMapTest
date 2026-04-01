using CMapTest.Core.Entities;
using CMapTest.Core.Extensions;
using FluentAssertions;

namespace CMapTest.Test.Extensions;

public class ProjectExtensionsTests
{
    [Fact]
    public void ToResponse_MapsIdAndName()
    {
        var entity = new ProjectEntity { Id = 42, Name = "Alpha Project" };

        var response = entity.ToResponse();

        response.Id.Should().Be(42);
        response.Name.Should().Be("Alpha Project");
    }

    [Fact]
    public void ToResponse_HandlesSpecialCharactersInName()
    {
        var entity = new ProjectEntity { Id = 1, Name = "Projet d'\u00e9t\u00e9 & <test> \"quotes\"" };

        var response = entity.ToResponse();

        response.Name.Should().Be("Projet d'\u00e9t\u00e9 & <test> \"quotes\"");
    }
}
