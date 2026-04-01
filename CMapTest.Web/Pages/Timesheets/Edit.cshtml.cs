using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMapTest.Web.Pages.Timesheets;

public class EditModel(
    ITimesheetService timesheetService,
    IUserService userService,
    IProjectService projectService) : PageModel
{
    [BindProperty]
    public UpdateTimesheetRequest Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public List<SelectListItem> UserOptions { get; set; } = [];
    public List<SelectListItem> ProjectOptions { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await timesheetService.GetByIdAsync(id);
        if (result.IsFailed)
            return RedirectToPage("Index");

        var ts = result.Value;
        Input = new UpdateTimesheetRequest
        {
            UserId = ts.UserId,
            ProjectId = ts.ProjectId,
            Date = ts.Date,
            Description = ts.Description,
            StartTime = ts.StartTime,
            EndTime = ts.EndTime
        };

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        var result = await timesheetService.UpdateAsync(id, Input);
        if (result.IsFailed)
        {
            await LoadDropdownsAsync();
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "Timesheet entry updated successfully.";
        return RedirectToPage("Index");
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
    }
}
