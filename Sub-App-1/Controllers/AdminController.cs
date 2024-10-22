using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sub_App_1.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller {
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(UserManager<IdentityUser> userManager) {
        _userManager = userManager;
    }

    // GET /Admin/UserManager
    public async Task<IActionResult> UserManager() {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // Additional admin actions like editing user roles, deleting users, etc. will be added here
}
