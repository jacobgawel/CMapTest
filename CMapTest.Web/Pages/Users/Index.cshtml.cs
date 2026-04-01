using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.User;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Users;

public class IndexModel(IUserService userService) : PageModel
{
    public PaginatedResponse<UserResponse>? Users { get; set; }

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var result = await userService.GetAllAsync(PageNumber, WebConstants.DefaultPageSize);
        if (result.IsSuccess)
            Users = result.Value;
    }
}
