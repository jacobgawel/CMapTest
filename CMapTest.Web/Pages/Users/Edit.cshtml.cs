using CMapTest.Core.DTOs;
using CMapTest.Core.DTOs.User;
using CMapTest.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMapTest.Web.Pages.Users;

public class EditModel(IUserService userService) : PageModel
{
    [BindProperty]
    public UpdateUserRequest Input { get; set; } = new() { Name = string.Empty };

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await userService.GetByIdAsync(id);
        if (result.IsFailed)
            return RedirectToPage("Index");

        Input = new UpdateUserRequest { Name = result.Value.Name };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await userService.UpdateAsync(id, Input);
        if (result.IsFailed)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            return Page();
        }

        TempData["StatusMessage"] = "User updated successfully.";
        return RedirectToPage("Index");
    }
}
