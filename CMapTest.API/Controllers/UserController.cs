using CMapTest.API.Extensions;
using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.User;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace CMapTest.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserResponse>>> GetAll([FromQuery] PaginationRequest pagination)
    {
        var result = await userService.GetAllAsync(pagination.Page, pagination.PageSize);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var result = await userService.GetByIdAsync(id);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var result = await userService.CreateAsync(request);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request)
    {
        var result = await userService.UpdateAsync(id, request);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await userService.DeleteAsync(id);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return NoContent();
    }
}
