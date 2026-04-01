using CMapTest.Core.DTOs;
using FluentAssertions;

namespace CMapTest.Test.DTOs;

public class PaginatedResponseTests
{
    [Fact]
    public void TotalPages_ReturnsOne_WhenTotalCountLessThanPageSize()
    {
        var response = new PaginatedResponse<string>
        {
            Items = ["a", "b"],
            Page = 1,
            PageSize = 10,
            TotalCount = 5
        };

        response.TotalPages.Should().Be(1);
    }

    [Fact]
    public void TotalPages_ReturnsCeiling_WhenNotEvenlyDivisible()
    {
        var response = new PaginatedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 11
        };

        response.TotalPages.Should().Be(2);
    }

    [Fact]
    public void TotalPages_ReturnsExactDivision_WhenEvenlyDivisible()
    {
        var response = new PaginatedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 20
        };

        response.TotalPages.Should().Be(2);
    }

    [Fact]
    public void TotalPages_ReturnsZero_WhenTotalCountIsZero()
    {
        var response = new PaginatedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public void TotalPages_ReturnsOne_WhenTotalCountEqualsOne()
    {
        var response = new PaginatedResponse<string>
        {
            Items = ["a"],
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        response.TotalPages.Should().Be(1);
    }

    [Fact]
    public void TotalPages_HandlesPageSizeOfOne()
    {
        var response = new PaginatedResponse<string>
        {
            Items = ["a"],
            Page = 1,
            PageSize = 1,
            TotalCount = 5
        };

        response.TotalPages.Should().Be(5);
    }

    [Fact]
    public void TotalPages_Throws_WhenPageSizeIsZero()
    {
        var response = new PaginatedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 0,
            TotalCount = 10
        };

        var act = () => response.TotalPages;

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("PageSize");
    }
}
