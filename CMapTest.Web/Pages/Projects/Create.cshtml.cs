using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Projects;

public class CreateModel(IProjectService projectService) : PageModel
{
    [BindProperty]
    public CreateProjectRequest Input { get; set; } = new() { Name = string.Empty };

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await projectService.CreateAsync(Input);
        if (result.IsFailed)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "Project created successfully.";
        return RedirectToPage("Index");
    }
}
