using CMapTest.Core.DTOs.User;
using CMapTest.Core.Entities;
using CMapTest.Core.Errors;
using CMapTest.Core.Exceptions;
using CMapTest.Core.Interfaces.Repository;
using CMapTest.Services;
using FluentAssertions;
using Moq;

namespace CMapTest.Test.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepo.Object, _timesheetRepo.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithPaginatedUserResponses()
    {
        var entities = new List<UserEntity>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        };
        _userRepo.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync((entities, 5));

        var result = await _sut.GetAllAsync(1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items[0].Id.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Alice");
        result.Value.Items[1].Id.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk_WithEmptyList_WhenNoUsers()
    {
        _userRepo.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync((new List<UserEntity>(), 0));

        var result = await _sut.GetAllAsync(1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ReturnsOk_WithUserResponse_WhenFound()
    {
        var entity = new UserEntity { Id = 7, Name = "Jane" };
        _userRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(7);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(7);
        result.Value.Name.Should().Be("Jane");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundError_WhenUserDoesNotExist()
    {
        _userRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((UserEntity?)null);

        var result = await _sut.GetByIdAsync(99);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
        result.Errors[0].Message.Should().Contain("99");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ReturnsOk_WithCreatedUserResponse()
    {
        var created = new UserEntity { Id = 10, Name = "New User" };
        _userRepo.Setup(r => r.CreateAsync(It.Is<UserEntity>(e => e.Name == "New User")))
            .ReturnsAsync(created);

        var result = await _sut.CreateAsync(new CreateUserRequest { Name = "New User" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(10);
        result.Value.Name.Should().Be("New User");
    }

    [Fact]
    public async Task CreateAsync_ReturnsFailure_WhenPersistenceExceptionThrown()
    {
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<UserEntity>()))
            .ThrowsAsync(new PersistenceException("db error", new Exception()));

        var result = await _sut.CreateAsync(new CreateUserRequest { Name = "Test" });

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Failed to save user");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ReturnsOk_WithUpdatedUserResponse()
    {
        var updated = new UserEntity { Id = 5, Name = "Updated Name" };
        _userRepo.Setup(r => r.UpdateAsync(5, It.IsAny<UserEntity>())).ReturnsAsync(updated);

        var result = await _sut.UpdateAsync(5, new UpdateUserRequest { Name = "Updated Name" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
        result.Value.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFoundError_WhenUserDoesNotExist()
    {
        _userRepo.Setup(r => r.UpdateAsync(99, It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _sut.UpdateAsync(99, new UpdateUserRequest { Name = "X" });

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenPersistenceExceptionThrown()
    {
        _userRepo.Setup(r => r.UpdateAsync(1, It.IsAny<UserEntity>()))
            .ThrowsAsync(new PersistenceException("db error", new Exception()));

        var result = await _sut.UpdateAsync(1, new UpdateUserRequest { Name = "X" });

        result.IsFailed.Should().BeTrue();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ReturnsOk_WhenDeletedSuccessfully()
    {
        _timesheetRepo.Setup(r => r.ExistsByUserIdAsync(1)).ReturnsAsync(false);
        _userRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsConflictError_WhenUserHasTimesheets()
    {
        _timesheetRepo.Setup(r => r.ExistsByUserIdAsync(1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>();
        _userRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFoundError_WhenUserDoesNotExist()
    {
        _timesheetRepo.Setup(r => r.ExistsByUserIdAsync(1)).ReturnsAsync(false);
        _userRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _sut.DeleteAsync(1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsConflictError_WhenPersistenceExceptionThrown()
    {
        _timesheetRepo.Setup(r => r.ExistsByUserIdAsync(1)).ReturnsAsync(false);
        _userRepo.Setup(r => r.DeleteAsync(1))
            .ThrowsAsync(new PersistenceException("FK constraint", new Exception()));

        var result = await _sut.DeleteAsync(1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>();
    }

    #endregion
}
