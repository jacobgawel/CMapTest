using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.User;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Users;

public class DeleteModel(IUserService userService) : PageModel
{
    public new UserResponse? User { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await userService.GetByIdAsync(id);
        if (result.IsFailed)
            return RedirectToPage("Index");

        User = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await userService.DeleteAsync(id);
        if (result.IsFailed)
        {
            var getResult = await userService.GetByIdAsync(id);
            if (getResult.IsSuccess)
                User = getResult.Value;

            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "User deleted successfully.";
        return RedirectToPage("Index");
    }
}
