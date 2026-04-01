using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMapTest.Web.Pages.Timesheets;

public class CreateModel(
    ITimesheetService timesheetService,
    IUserService userService,
    IProjectService projectService) : PageModel
{
    [BindProperty]
    public CreateTimesheetRequest Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public List<SelectListItem> UserOptions { get; set; } = [];
    public List<SelectListItem> ProjectOptions { get; set; } = [];

    public async Task OnGetAsync()
    {
        Input.Date = DateOnly.FromDateTime(DateTime.Today);
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        var result = await timesheetService.CreateAsync(Input);
        if (result.IsFailed)
        {
            await LoadDropdownsAsync();
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "Timesheet entry created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var usersResult = await userService.GetAllAsync(1, 100);
        if (usersResult.IsSuccess)
            UserOptions = usersResult.Value.Items
                .Select(u => new SelectListItem(u.Name, u.Id.ToString()))
                .ToList();

        var projectsResult = await projectService.GetAllAsync(1, 100);
        if (projectsResult.IsSuccess)
            ProjectOptions = projectsResult.Value.Items
                .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                .ToList();
    }
}
