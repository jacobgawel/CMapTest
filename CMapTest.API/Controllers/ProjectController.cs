using CMapTest.API.Extensions;
using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace CMapTest.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProjectResponse>>> GetAll([FromQuery] PaginationRequest pagination)
    {
        var result = await projectService.GetAllAsync(pagination.Page, pagination.PageSize);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id)
    {
        var result = await projectService.GetByIdAsync(id);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request)
    {
        var result = await projectService.CreateAsync(request);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> Update(int id, UpdateProjectRequest request)
    {
        var result = await projectService.UpdateAsync(id, request);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await projectService.DeleteAsync(id);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return NoContent();
    }
}
