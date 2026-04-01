using CMapTest.Core.Entities;
using CMapTest.Core.Extensions;
using FluentAssertions;

namespace CMapTest.Test.Extensions;

public class TimesheetExtensionsTests
{
    [Fact]
    public void ToResponse_MapsAllScalarFields()
    {
        var entity = new TimesheetEntity
        {
            Id = 10,
            UserId = 3,
            ProjectId = 5,
            Date = new DateOnly(2025, 6, 15),
            Description = "Worked on feature",
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        var response = entity.ToResponse();

        response.Id.Should().Be(10);
        response.UserId.Should().Be(3);
        response.ProjectId.Should().Be(5);
        response.Date.Should().Be(new DateOnly(2025, 6, 15));
        response.Description.Should().Be("Worked on feature");
        response.StartTime.Should().Be(new TimeOnly(9, 0));
        response.EndTime.Should().Be(new TimeOnly(17, 0));
    }

    [Fact]
    public void ToResponse_CalculatesHoursWorked_ForWholeHours()
    {
        var entity = new TimesheetEntity
        {
            Id = 1,
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 1, 1),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            User = new UserEntity { Name = "Test" },
            Project = new ProjectEntity { Name = "Test" }
        };

        var response = entity.ToResponse();

        response.HoursWorked.Should().Be(8.0m);
    }

    [Fact]
    public void ToResponse_CalculatesHoursWorked_ForFractionalHours()
    {
        var entity = new TimesheetEntity
        {
            Id = 1,
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 1, 1),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30)
        };

        var response = entity.ToResponse();

        response.HoursWorked.Should().Be(1.5m);
    }

    [Fact]
    public void ToResponse_CalculatesHoursWorked_ForSmallDuration()
    {
        var entity = new TimesheetEntity
        {
            Id = 1,
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 1, 1),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(14, 15)
        };

        var response = entity.ToResponse();

        response.HoursWorked.Should().Be(0.25m);
    }

    [Fact]
    public void ToResponse_SetsDuration_AsHumanizedString()
    {
        var entity = new TimesheetEntity
        {
            Id = 1,
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 1, 1),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        var response = entity.ToResponse();

        response.Duration.Should().NotBeNullOrEmpty();
        response.Duration.Should().Contain("8");
    }

    [Fact]
    public void ToResponse_HandlesNullDescription()
    {
        var entity = new TimesheetEntity
        {
            Id = 1,
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 1, 1),
            Description = null,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        var response = entity.ToResponse();

        response.Description.Should().BeNull();
    }

    [Fact]
    public void ToResponse_HandlesNonNullDescription()
    {
        var entity = new TimesheetEntity
        {
            Id = 1,
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 1, 1),
            Description = "Some work",
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        var response = entity.ToResponse();

        response.Description.Should().Be("Some work");
    }
}
