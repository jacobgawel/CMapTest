using CMapTest.Core.Entities;
using CMapTest.Core.Extensions;
using FluentAssertions;

namespace CMapTest.Test.Extensions;

public class UserExtensionsTests
{
    [Fact]
    public void ToResponse_MapsIdAndName()
    {
        var entity = new UserEntity { Id = 7, Name = "Jane Doe" };

        var response = entity.ToResponse();

        response.Id.Should().Be(7);
        response.Name.Should().Be("Jane Doe");
    }
}
