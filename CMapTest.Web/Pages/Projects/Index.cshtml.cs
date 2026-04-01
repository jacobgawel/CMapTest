using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.Project;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Projects;

public class IndexModel(IProjectService projectService) : PageModel
{
    public PaginatedResponse<ProjectResponse>? Projects { get; set; }

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var result = await projectService.GetAllAsync(PageNumber, 20);
        if (result.IsSuccess)
            Projects = result.Value;
    }
}
