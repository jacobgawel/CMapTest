using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Projects;

public class EditModel(IProjectService projectService) : PageModel
{
    [BindProperty]
    public UpdateProjectRequest Input { get; set; } = new() { Name = string.Empty };

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await projectService.GetByIdAsync(id);
        if (result.IsFailed)
            return RedirectToPage("Index");

        Input = new UpdateProjectRequest { Name = result.Value.Name };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await projectService.UpdateAsync(id, Input);
        if (result.IsFailed)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "Project updated successfully.";
        return RedirectToPage("Index");
    }
}
