using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMapTest.Web.Pages.Timesheets;

public class IndexModel(
    ITimesheetService timesheetService,
    IUserService userService,
    IProjectService projectService) : PageModel
{
    public PaginatedResponse<TimesheetResponse>? Timesheets { get; set; }

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public int? UserId { get; set; }

    [FromQuery]
    public int? ProjectId { get; set; }

    [FromQuery]
    public DateOnly? Week { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public List<SelectListItem> UserOptions { get; set; } = [];
    public List<SelectListItem> ProjectOptions { get; set; } = [];
    public List<SelectListItem> WeekOptions { get; set; } = [];
    public List<ProjectHoursSummary> ProjectHoursSummaries { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();

        DateOnly? startDate = null;
        DateOnly? endDate = null;

        if (Week.HasValue)
        {
            startDate = Week.Value;
            endDate = Week.Value.AddDays(6);
        }

        var result = await timesheetService.GetFilteredAsync(UserId, ProjectId, startDate, endDate, PageNumber, WebConstants.TimesheetPageSize);
        if (result.IsSuccess)
            Timesheets = result.Value;

        if (startDate.HasValue || UserId.HasValue || ProjectId.HasValue)
        {
            var summaryResult = await timesheetService.GetProjectHoursSummaryAsync(UserId, ProjectId, startDate, endDate);
            if (summaryResult.IsSuccess)
                ProjectHoursSummaries = summaryResult.Value;
        }
    }

    private async Task LoadDropdownsAsync()
    {
        var usersResult = await userService.GetAllAsync(1, WebConstants.DropdownMaxItems);
        if (usersResult.IsSuccess)
            UserOptions = usersResult.Value.Items
                .Select(u => new SelectListItem(u.Name, u.Id.ToString()))
                .ToList();

        var projectsResult = await projectService.GetAllAsync(1, WebConstants.DropdownMaxItems);
        if (projectsResult.IsSuccess)
            ProjectOptions = projectsResult.Value.Items
                .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                .ToList();

        var dateRangeResult = await timesheetService.GetDateRangeAsync();
        if (dateRangeResult.IsSuccess && dateRangeResult.Value.MinDate.HasValue && dateRangeResult.Value.MaxDate.HasValue)
        {
            var minMonday = GetMonday(dateRangeResult.Value.MinDate.Value);
            var maxMonday = GetMonday(dateRangeResult.Value.MaxDate.Value);

            for (var monday = maxMonday; monday >= minMonday; monday = monday.AddDays(-7))
            {
                var sunday = monday.AddDays(6);
                var label = $"{monday:MMM dd} - {sunday:MMM dd, yyyy}";
                WeekOptions.Add(new SelectListItem(label, monday.ToString("yyyy-MM-dd")));
            }
        }
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        var daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysFromMonday);
    }
}
