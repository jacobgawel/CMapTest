using CMapTest.API.Extensions;
using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace CMapTest.API.Controllers;

[ApiController]
[Route("api/timesheets")]
public class TimesheetController(ITimesheetService timesheetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<TimesheetResponse>>> GetAll(
        [FromQuery] int? userId,
        [FromQuery] int? projectId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] PaginationRequest pagination)
    {
        var result = await timesheetService.GetFilteredAsync(userId, projectId, startDate, endDate, pagination.Page, pagination.PageSize);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpGet("summary/hours-per-project")]
    public async Task<ActionResult<List<ProjectHoursSummary>>> GetHoursPerProject(
        [FromQuery] int? userId,
        [FromQuery] int? projectId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
    {
        var result = await timesheetService.GetProjectHoursSummaryAsync(userId, projectId, startDate, endDate);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimesheetResponse>> GetById(int id)
    {
        var result = await timesheetService.GetByIdAsync(id);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<TimesheetResponse>> Create(CreateTimesheetRequest request)
    {
        var result = await timesheetService.CreateAsync(request);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TimesheetResponse>> Update(int id, UpdateTimesheetRequest request)
    {
        var result = await timesheetService.UpdateAsync(id, request);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await timesheetService.DeleteAsync(id);
        if (result.IsFailed) return result.ToErrorResponse(this);
        return NoContent();
    }
}
