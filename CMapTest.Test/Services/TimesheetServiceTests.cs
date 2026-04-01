using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Entities;
using CMapTest.Core.Errors;
using CMapTest.Core.Exceptions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Services;
using FluentAssertions;
using Moq;

namespace CMapTest.Test.Services;

public class TimesheetServiceTests
{
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly TimesheetService _sut;

    private static readonly DateTimeOffset FixedUtcNow = new(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);

    // Valid defaults for a timesheet that passes all validation
    private static readonly DateOnly ValidDate = DateOnly.FromDateTime(FixedUtcNow.UtcDateTime);
    private static readonly TimeOnly ValidStartTime = new(9, 0);
    private static readonly TimeOnly ValidEndTime = new(17, 0);
    private const int ValidUserId = 1;
    private const int ValidProjectId = 1;

    public TimesheetServiceTests()
    {
        _timeProvider.Setup(tp => tp.GetUtcNow()).Returns(FixedUtcNow);
        _sut = new TimesheetService(_timesheetRepo.Object, _userRepo.Object, _projectRepo.Object, _timeProvider.Object);
    }

    private void SetupValidationToPass()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new UserEntity { Id = ValidUserId, Name = "Test User" });
        _projectRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ProjectEntity { Id = ValidProjectId, Name = "Test Project" });
        _timesheetRepo.Setup(r => r.HasOverlappingEntryAsync(
                It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
    }

    private CreateTimesheetRequest MakeValidCreateRequest() => new()
    {
        UserId = ValidUserId,
        ProjectId = ValidProjectId,
        Date = ValidDate,
        Description = "Test work",
        StartTime = ValidStartTime,
        EndTime = ValidEndTime
    };

    private UpdateTimesheetRequest MakeValidUpdateRequest() => new()
    {
        UserId = ValidUserId,
        ProjectId = ValidProjectId,
        Date = ValidDate,
        Description = "Updated work",
        StartTime = ValidStartTime,
        EndTime = ValidEndTime
    };

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithPaginatedTimesheetResponses()
    {
        var entities = new List<TimesheetEntity>
        {
            new()
            {
                Id = 1, UserId = 1, ProjectId = 1,
                Date = new DateOnly(2025, 1, 1),
                StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0),
                User = new UserEntity { Name = "U" }, Project = new ProjectEntity { Name = "P" }
            }
        };
        _timesheetRepo.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync((entities, 1));

        var result = await _sut.GetAllAsync(1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Id.Should().Be(1);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithEmptyList_WhenNoTimesheets()
    {
        _timesheetRepo.Setup(r => r.GetAllAsync(1, 10))
            .ReturnsAsync((new List<TimesheetEntity>(), 0));

        var result = await _sut.GetAllAsync(1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ReturnsOk_WithTimesheetResponse_WhenFound()
    {
        var entity = new TimesheetEntity
        {
            Id = 5, UserId = 1, ProjectId = 2,
            Date = new DateOnly(2025, 3, 10),
            StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0)
        };
        _timesheetRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
        result.Value.UserId.Should().Be(1);
        result.Value.ProjectId.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundError_WhenTimesheetDoesNotExist()
    {
        _timesheetRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TimesheetEntity?)null);

        var result = await _sut.GetByIdAsync(99);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
        result.Errors[0].Message.Should().Contain("99");
    }

    #endregion

    #region GetFilteredAsync

    [Fact]
    public async Task GetFilteredAsync_ReturnsOk_WithFilteredPaginatedResults()
    {
        var entities = new List<TimesheetEntity>
        {
            new()
            {
                Id = 1, UserId = 2, ProjectId = 3,
                Date = new DateOnly(2025, 5, 1),
                StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0)
            }
        };
        _timesheetRepo.Setup(r => r.GetFilteredAsync(2, 3, It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), 1, 10))
            .ReturnsAsync((entities, 1));

        var result = await _sut.GetFilteredAsync(2, 3, null, null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetFilteredAsync_ReturnsOk_WithNullFilters()
    {
        _timesheetRepo.Setup(r => r.GetFilteredAsync(null, null, null, null, 1, 10))
            .ReturnsAsync((new List<TimesheetEntity>(), 0));

        var result = await _sut.GetFilteredAsync(null, null, null, null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    #endregion

    #region GetProjectHoursSummaryAsync

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ReturnsOk_WithMappedSummaries()
    {
        var data = new List<(int ProjectId, string ProjectName, decimal TotalHours)>
        {
            (1, "Project A", 10.5m),
            (2, "Project B", 20.0m)
        };
        _timesheetRepo.Setup(r => r.GetProjectHoursSummaryAsync(null, null, null, null))
            .ReturnsAsync(data);

        var result = await _sut.GetProjectHoursSummaryAsync(null, null, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].ProjectId.Should().Be(1);
        result.Value[0].ProjectName.Should().Be("Project A");
        result.Value[0].TotalHours.Should().Be(10.5m);
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_RoundsTotalHoursToTwoDecimals()
    {
        var data = new List<(int ProjectId, string ProjectName, decimal TotalHours)>
        {
            (1, "Test", 1.23456m)
        };
        _timesheetRepo.Setup(r => r.GetProjectHoursSummaryAsync(null, null, null, null))
            .ReturnsAsync(data);

        var result = await _sut.GetProjectHoursSummaryAsync(null, null, null, null);

        result.Value[0].TotalHours.Should().Be(1.23m);
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ReturnsEmptyList_WhenNoData()
    {
        _timesheetRepo.Setup(r => r.GetProjectHoursSummaryAsync(null, null, null, null))
            .ReturnsAsync(new List<(int, string, decimal)>());

        var result = await _sut.GetProjectHoursSummaryAsync(null, null, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region GetDateRangeAsync

    [Fact]
    public async Task GetDateRangeAsync_ReturnsOk_WithDateRange()
    {
        var min = new DateOnly(2025, 1, 1);
        var max = new DateOnly(2025, 12, 31);
        _timesheetRepo.Setup(r => r.GetDateRangeAsync()).ReturnsAsync((min, max));

        var result = await _sut.GetDateRangeAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.MinDate.Should().Be(min);
        result.Value.MaxDate.Should().Be(max);
    }

    [Fact]
    public async Task GetDateRangeAsync_ReturnsOk_WithNullDates_WhenNoTimesheets()
    {
        _timesheetRepo.Setup(r => r.GetDateRangeAsync())
            .ReturnsAsync(((DateOnly?)null, (DateOnly?)null));

        var result = await _sut.GetDateRangeAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.MinDate.Should().BeNull();
        result.Value.MaxDate.Should().BeNull();
    }

    #endregion

    #region CreateAsync - Validation

    [Fact]
    public async Task CreateAsync_ReturnsValidationError_WhenEndTimeEqualsStartTime()
    {
        var request = MakeValidCreateRequest();
        request.StartTime = new TimeOnly(9, 0);
        request.EndTime = new TimeOnly(9, 0);

        var result = await _sut.CreateAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
        result.Errors[0].Message.Should().Contain("End time must be after start time");
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationError_WhenEndTimeBeforeStartTime()
    {
        var request = MakeValidCreateRequest();
        request.StartTime = new TimeOnly(17, 0);
        request.EndTime = new TimeOnly(9, 0);

        var result = await _sut.CreateAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
        result.Errors[0].Message.Should().Contain("End time must be after start time");
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationError_WhenDateIsInFuture()
    {
        var request = MakeValidCreateRequest();
        request.Date = DateOnly.FromDateTime(FixedUtcNow.UtcDateTime).AddDays(1);

        var result = await _sut.CreateAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
        result.Errors[0].Message.Should().Contain("Date cannot be in the future");
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationError_WhenUserDoesNotExist()
    {
        SetupValidationToPass();
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((UserEntity?)null);

        var request = MakeValidCreateRequest();
        var result = await _sut.CreateAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
        result.Errors[0].Message.Should().Contain($"User with id {ValidUserId} not found");
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationError_WhenProjectDoesNotExist()
    {
        SetupValidationToPass();
        _projectRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ProjectEntity?)null);

        var request = MakeValidCreateRequest();
        var result = await _sut.CreateAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
        result.Errors[0].Message.Should().Contain($"Project with id {ValidProjectId} not found");
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflictError_WhenOverlappingEntryExists()
    {
        SetupValidationToPass();
        _timesheetRepo.Setup(r => r.HasOverlappingEntryAsync(
                It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(true);

        var request = MakeValidCreateRequest();
        var result = await _sut.CreateAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>();
        result.Errors[0].Message.Should().Contain("overlaps");
    }

    [Fact]
    public async Task CreateAsync_CallsHasOverlappingEntryAsync_WithNullExcludeId()
    {
        SetupValidationToPass();
        var created = new TimesheetEntity
        {
            Id = 1, UserId = ValidUserId, ProjectId = ValidProjectId,
            Date = ValidDate, StartTime = ValidStartTime, EndTime = ValidEndTime
        };
        _timesheetRepo.Setup(r => r.CreateAsync(It.IsAny<TimesheetEntity>())).ReturnsAsync(created);

        await _sut.CreateAsync(MakeValidCreateRequest());

        _timesheetRepo.Verify(r => r.HasOverlappingEntryAsync(
            ValidUserId, ValidDate, ValidStartTime, ValidEndTime, null), Times.Once());
    }

    #endregion

    #region CreateAsync - Happy Path & Failures

    [Fact]
    public async Task CreateAsync_ReturnsOk_WithCreatedTimesheetResponse_WhenValid()
    {
        SetupValidationToPass();
        var created = new TimesheetEntity
        {
            Id = 10, UserId = ValidUserId, ProjectId = ValidProjectId,
            Date = ValidDate, Description = "Test work",
            StartTime = ValidStartTime, EndTime = ValidEndTime
        };
        _timesheetRepo.Setup(r => r.CreateAsync(It.IsAny<TimesheetEntity>())).ReturnsAsync(created);

        var result = await _sut.CreateAsync(MakeValidCreateRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);
        result.Value.UserId.Should().Be(ValidUserId);
        result.Value.ProjectId.Should().Be(ValidProjectId);
    }

    [Fact]
    public async Task CreateAsync_ReturnsFailure_WhenPersistenceExceptionThrown()
    {
        SetupValidationToPass();
        _timesheetRepo.Setup(r => r.CreateAsync(It.IsAny<TimesheetEntity>()))
            .ThrowsAsync(new PersistenceException("db error", new Exception()));

        var result = await _sut.CreateAsync(MakeValidCreateRequest());

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Failed to save timesheet");
    }

    [Fact]
    public async Task CreateAsync_PassesCorrectEntityToRepository()
    {
        SetupValidationToPass();
        var created = new TimesheetEntity
        {
            Id = 1, UserId = ValidUserId, ProjectId = ValidProjectId,
            Date = ValidDate, Description = "Test work",
            StartTime = ValidStartTime, EndTime = ValidEndTime
        };
        _timesheetRepo.Setup(r => r.CreateAsync(It.IsAny<TimesheetEntity>())).ReturnsAsync(created);

        var request = MakeValidCreateRequest();
        await _sut.CreateAsync(request);

        _timesheetRepo.Verify(r => r.CreateAsync(It.Is<TimesheetEntity>(e =>
            e.UserId == request.UserId &&
            e.ProjectId == request.ProjectId &&
            e.Date == request.Date &&
            e.Description == request.Description &&
            e.StartTime == request.StartTime &&
            e.EndTime == request.EndTime
        )), Times.Once());
    }

    #endregion

    #region UpdateAsync - Validation

    [Fact]
    public async Task UpdateAsync_ReturnsValidationError_WhenEndTimeBeforeStartTime()
    {
        var request = MakeValidUpdateRequest();
        request.StartTime = new TimeOnly(17, 0);
        request.EndTime = new TimeOnly(9, 0);

        var result = await _sut.UpdateAsync(1, request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsValidationError_WhenDateIsInFuture()
    {
        var request = MakeValidUpdateRequest();
        request.Date = DateOnly.FromDateTime(FixedUtcNow.UtcDateTime).AddDays(1);

        var result = await _sut.UpdateAsync(1, request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsValidationError_WhenUserDoesNotExist()
    {
        SetupValidationToPass();
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((UserEntity?)null);

        var result = await _sut.UpdateAsync(1, MakeValidUpdateRequest());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsValidationError_WhenProjectDoesNotExist()
    {
        SetupValidationToPass();
        _projectRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ProjectEntity?)null);

        var result = await _sut.UpdateAsync(1, MakeValidUpdateRequest());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsConflictError_WhenOverlappingEntryExists()
    {
        SetupValidationToPass();
        _timesheetRepo.Setup(r => r.HasOverlappingEntryAsync(
                It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateAsync(1, MakeValidUpdateRequest());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>();
    }

    [Fact]
    public async Task UpdateAsync_CallsHasOverlappingEntryAsync_WithExcludeId()
    {
        SetupValidationToPass();
        var updated = new TimesheetEntity
        {
            Id = 42, UserId = ValidUserId, ProjectId = ValidProjectId,
            Date = ValidDate, StartTime = ValidStartTime, EndTime = ValidEndTime
        };
        _timesheetRepo.Setup(r => r.UpdateAsync(42, It.IsAny<TimesheetEntity>())).ReturnsAsync(updated);

        await _sut.UpdateAsync(42, MakeValidUpdateRequest());

        _timesheetRepo.Verify(r => r.HasOverlappingEntryAsync(
            ValidUserId, ValidDate, ValidStartTime, ValidEndTime, 42), Times.Once());
    }

    #endregion

    #region UpdateAsync - Happy Path & Failures

    [Fact]
    public async Task UpdateAsync_ReturnsOk_WithUpdatedTimesheetResponse_WhenValid()
    {
        SetupValidationToPass();
        var updated = new TimesheetEntity
        {
            Id = 5, UserId = ValidUserId, ProjectId = ValidProjectId,
            Date = ValidDate, Description = "Updated work",
            StartTime = ValidStartTime, EndTime = ValidEndTime
        };
        _timesheetRepo.Setup(r => r.UpdateAsync(5, It.IsAny<TimesheetEntity>())).ReturnsAsync(updated);

        var result = await _sut.UpdateAsync(5, MakeValidUpdateRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFoundError_WhenTimesheetDoesNotExist()
    {
        SetupValidationToPass();
        _timesheetRepo.Setup(r => r.UpdateAsync(99, It.IsAny<TimesheetEntity>()))
            .ReturnsAsync((TimesheetEntity?)null);

        var result = await _sut.UpdateAsync(99, MakeValidUpdateRequest());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenPersistenceExceptionThrown()
    {
        SetupValidationToPass();
        _timesheetRepo.Setup(r => r.UpdateAsync(1, It.IsAny<TimesheetEntity>()))
            .ThrowsAsync(new PersistenceException("db error", new Exception()));

        var result = await _sut.UpdateAsync(1, MakeValidUpdateRequest());

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Failed to save timesheet");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ReturnsOk_WhenDeletedSuccessfully()
    {
        _timesheetRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFoundError_WhenTimesheetDoesNotExist()
    {
        _timesheetRepo.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _sut.DeleteAsync(99);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenPersistenceExceptionThrown()
    {
        _timesheetRepo.Setup(r => r.DeleteAsync(1))
            .ThrowsAsync(new PersistenceException("db error", new Exception()));

        var result = await _sut.DeleteAsync(1);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Failed to delete timesheet");
    }

    #endregion

    #region Validation Order Verification

    [Fact]
    public async Task CreateAsync_DoesNotCheckUserExistence_WhenTimeValidationFails()
    {
        var request = MakeValidCreateRequest();
        request.StartTime = new TimeOnly(17, 0);
        request.EndTime = new TimeOnly(9, 0);

        await _sut.CreateAsync(request);

        _userRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never());
    }

    [Fact]
    public async Task CreateAsync_DoesNotCheckProjectExistence_WhenUserDoesNotExist()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((UserEntity?)null);

        await _sut.CreateAsync(MakeValidCreateRequest());

        _projectRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never());
    }

    [Fact]
    public async Task CreateAsync_DoesNotCheckOverlap_WhenProjectDoesNotExist()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new UserEntity { Id = 1, Name = "Test" });
        _projectRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ProjectEntity?)null);

        await _sut.CreateAsync(MakeValidCreateRequest());

        _timesheetRepo.Verify(r => r.HasOverlappingEntryAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()),
            Times.Never());
    }

    [Fact]
    public async Task CreateAsync_AcceptsDateOfToday()
    {
        SetupValidationToPass();
        var created = new TimesheetEntity
        {
            Id = 1, UserId = ValidUserId, ProjectId = ValidProjectId,
            Date = ValidDate, StartTime = ValidStartTime, EndTime = ValidEndTime
        };
        _timesheetRepo.Setup(r => r.CreateAsync(It.IsAny<TimesheetEntity>())).ReturnsAsync(created);

        var request = MakeValidCreateRequest();
        request.Date = DateOnly.FromDateTime(FixedUtcNow.UtcDateTime);

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
