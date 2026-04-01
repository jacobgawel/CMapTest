using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Entities;
using CMapTest.Core.Errors;
using CMapTest.Core.Exceptions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Services;
using FluentAssertions;
using Moq;

namespace CMapTest.Test.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _sut = new ProjectService(_projectRepo.Object, _timesheetRepo.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithPaginatedProjectResponses()
    {
        var entities = new List<ProjectEntity>
        {
            new() { Id = 1, Name = "Project A" },
            new() { Id = 2, Name = "Project B" }
        };
        _projectRepo.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync((entities, 5));

        var result = await _sut.GetAllAsync(1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items[0].Id.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Project A");
        result.Value.Items[1].Id.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithEmptyList_WhenNoProjects()
    {
        _projectRepo.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync((new List<ProjectEntity>(), 0));

        var result = await _sut.GetAllAsync(1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ReturnsOk_WithProjectResponse_WhenFound()
    {
        var entity = new ProjectEntity { Id = 42, Name = "Alpha" };
        _projectRepo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(42);
        result.Value.Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundError_WhenProjectDoesNotExist()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ProjectEntity?)null);

        var result = await _sut.GetByIdAsync(99);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
        result.Errors[0].Message.Should().Contain("99");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ReturnsOk_WithCreatedProjectResponse()
    {
        var created = new ProjectEntity { Id = 10, Name = "New Project" };
        _projectRepo.Setup(r => r.CreateAsync(It.Is<ProjectEntity>(e => e.Name == "New Project")))
            .ReturnsAsync(created);

        var result = await _sut.CreateAsync(new CreateProjectRequest { Name = "New Project" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);
        result.Value.Name.Should().Be("New Project");
    }

    [Fact]
    public async Task CreateAsync_ReturnsFailure_WhenPersistenceExceptionThrown()
    {
        _projectRepo.Setup(r => r.CreateAsync(It.IsAny<ProjectEntity>()))
            .ThrowsAsync(new PersistenceException("db error", new Exception()));

        var result = await _sut.CreateAsync(new CreateProjectRequest { Name = "Test" });

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Failed to save project");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ReturnsOk_WithUpdatedProjectResponse()
    {
        var updated = new ProjectEntity { Id = 5, Name = "Updated Name" };
        _projectRepo.Setup(r => r.UpdateAsync(5, It.IsAny<ProjectEntity>())).ReturnsAsync(updated);

        var result = await _sut.UpdateAsync(5, new UpdateProjectRequest { Name = "Updated Name" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
        result.Value.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFoundError_WhenProjectDoesNotExist()
    {
        _projectRepo.Setup(r => r.UpdateAsync(99, It.IsAny<ProjectEntity>()))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _sut.UpdateAsync(99, new UpdateProjectRequest { Name = "X" });

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenPersistenceExceptionThrown()
    {
        _projectRepo.Setup(r => r.UpdateAsync(1, It.IsAny<ProjectEntity>()))
            .ThrowsAsync(new PersistenceException("db error", new Exception()));

        var result = await _sut.UpdateAsync(1, new UpdateProjectRequest { Name = "X" });

        result.IsFailed.Should().BeTrue();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ReturnsOk_WhenDeletedSuccessfully()
    {
        _timesheetRepo.Setup(r => r.ExistsByProjectIdAsync(1)).ReturnsAsync(false);
        _projectRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsConflictError_WhenProjectHasTimesheets()
    {
        _timesheetRepo.Setup(r => r.ExistsByProjectIdAsync(1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>();
        _projectRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFoundError_WhenProjectDoesNotExist()
    {
        _timesheetRepo.Setup(r => r.ExistsByProjectIdAsync(1)).ReturnsAsync(false);
        _projectRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _sut.DeleteAsync(1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsConflictError_WhenPersistenceExceptionThrown()
    {
        _timesheetRepo.Setup(r => r.ExistsByProjectIdAsync(1)).ReturnsAsync(false);
        _projectRepo.Setup(r => r.DeleteAsync(1))
            .ThrowsAsync(new PersistenceException("FK constraint", new Exception()));

        var result = await _sut.DeleteAsync(1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>();
    }

    #endregion
}
