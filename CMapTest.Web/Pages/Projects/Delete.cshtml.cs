using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Projects;

public class DeleteModel(IProjectService projectService) : PageModel
{
    public ProjectResponse? Project { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await projectService.GetByIdAsync(id);
        if (result.IsFailed)
            return RedirectToPage("Index");

        Project = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await projectService.DeleteAsync(id);
        if (result.IsFailed)
        {
            var getResult = await projectService.GetByIdAsync(id);
            if (getResult.IsSuccess)
                Project = getResult.Value;

            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "Project deleted successfully.";
        return RedirectToPage("Index");
    }
}
