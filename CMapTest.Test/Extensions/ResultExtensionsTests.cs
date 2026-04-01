using CMapTest.API.Extensions;
using CMapTest.Core.DTOs;
using CMapTest.Core.Errors;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CMapTest.Test.Extensions;

public class ResultExtensionsTests
{
    private static ControllerBase CreateController()
    {
        var controller = new TestController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    private class TestController : ControllerBase { }

    [Fact]
    public void ToErrorResponse_Returns404NotFound_WhenResultContainsNotFoundError()
    {
        var result = Result.Fail(new NotFoundError("Item not found"));
        var controller = CreateController();

        var actionResult = result.ToErrorResponse(controller);

        var notFound = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        var body = notFound.Value.Should().BeOfType<ErrorResponse>().Subject;
        body.Type.Should().Be("NotFound");
        body.Errors.Should().Contain("Item not found");
    }

    [Fact]
    public void ToErrorResponse_Returns409Conflict_WhenResultContainsConflictError()
    {
        var result = Result.Fail(new ConflictError("Resource conflict"));
        var controller = CreateController();

        var actionResult = result.ToErrorResponse(controller);

        var conflict = actionResult.Should().BeOfType<ConflictObjectResult>().Subject;
        var body = conflict.Value.Should().BeOfType<ErrorResponse>().Subject;
        body.Type.Should().Be("Conflict");
        body.Errors.Should().Contain("Resource conflict");
    }

    [Fact]
    public void ToErrorResponse_Returns422UnprocessableEntity_WhenResultContainsValidationError()
    {
        var result = Result.Fail(new ValidationError("Invalid input"));
        var controller = CreateController();

        var actionResult = result.ToErrorResponse(controller);

        var unprocessable = actionResult.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
        var body = unprocessable.Value.Should().BeOfType<ErrorResponse>().Subject;
        body.Type.Should().Be("ValidationError");
        body.Errors.Should().Contain("Invalid input");
    }

    [Fact]
    public void ToErrorResponse_Returns400BadRequest_WhenResultContainsGenericError()
    {
        var result = Result.Fail("Something went wrong");
        var controller = CreateController();

        var actionResult = result.ToErrorResponse(controller);

        var badRequest = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        var body = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        body.Type.Should().Be("BadRequest");
        body.Errors.Should().Contain("Something went wrong");
    }

    [Fact]
    public void ToErrorResponse_CollectsAllErrorMessages()
    {
        var result = Result.Fail(new NotFoundError("Error one"))
            .WithError(new NotFoundError("Error two"));
        var controller = CreateController();

        var actionResult = result.ToErrorResponse(controller);

        var notFound = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        var body = notFound.Value.Should().BeOfType<ErrorResponse>().Subject;
        body.Errors.Should().HaveCount(2);
        body.Errors.Should().Contain("Error one");
        body.Errors.Should().Contain("Error two");
    }

    [Fact]
    public void ToErrorResponse_PrioritizesNotFoundError_WhenMultipleErrorTypesPresent()
    {
        var result = Result.Fail(new NotFoundError("Not found"))
            .WithError("Generic error");
        var controller = CreateController();

        var actionResult = result.ToErrorResponse(controller);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void ToErrorResponse_PrioritizesConflictError_OverValidationError()
    {
        var result = Result.Fail(new ConflictError("Conflict"))
            .WithError(new ValidationError("Validation"));
        var controller = CreateController();

        var actionResult = result.ToErrorResponse(controller);

        actionResult.Should().BeOfType<ConflictObjectResult>();
    }
}
