using CMapTest.Core.DTOs;
using CMapTest.Core.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace CMapTest.API.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToErrorResponse(this ResultBase result, ControllerBase controller)
    {
        var messages = result.Errors.Select(e => e.Message).ToList();

        if (result.Errors.OfType<NotFoundError>().Any())
            return controller.NotFound(new ErrorResponse { Type = "NotFound", Errors = messages });
        if (result.Errors.OfType<ConflictError>().Any())
            return controller.Conflict(new ErrorResponse { Type = "Conflict", Errors = messages });
        if (result.Errors.OfType<ValidationError>().Any())
            return controller.UnprocessableEntity(new ErrorResponse { Type = "ValidationError", Errors = messages });

        return controller.BadRequest(new ErrorResponse { Type = "BadRequest", Errors = messages });
    }
}
