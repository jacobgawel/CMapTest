using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Timesheet;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Timesheets;

public class DeleteModel(ITimesheetService timesheetService) : PageModel
{
    public TimesheetResponse? Timesheet { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await timesheetService.GetByIdAsync(id);
        if (result.IsFailed)
            return RedirectToPage("Index");

        Timesheet = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await timesheetService.DeleteAsync(id);
        if (result.IsFailed)
        {
            var getResult = await timesheetService.GetByIdAsync(id);
            if (getResult.IsSuccess)
                Timesheet = getResult.Value;

            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "Timesheet entry deleted successfully.";
        return RedirectToPage("Index");
    }
}
