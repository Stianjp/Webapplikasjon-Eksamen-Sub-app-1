using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sub_App_1.Models;

[Authorize(Roles = UserRoles.Administrator)]
public class AdminController : Controller {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET /Admin/UserManager
    public async Task<IActionResult> UserManager() {
        var users = _userManager.Users.ToList();
        var userWithRoles = new List<UserWithRolesViewModel>();

        foreach (var user in users) {
            var roles = await _userManager.GetRolesAsync(user);
            userWithRoles.Add(new UserWithRolesViewModel {
                UserId = user.Id,
                Username = user.UserName,
                Roles = roles
            });
        }

        return View(userWithRoles); // pass the list of users with their roles to the view
    }

    // GET /Admin/EditUser/{id}
    public async Task<IActionResult> EditUser(string id) {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) {
            return NotFound();
        }

        var model = new UserWithRolesViewModel {
            UserId = user.Id,
            Username = user.UserName,
            Roles = await _userManager.GetRolesAsync(user)  // fetch current roles
        };

        ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();  // pass all roles to the view for selection
        return View(model); 
    }

    // POST /Admin/EditUser
    [HttpPost]
    public async Task<IActionResult> EditUser(UserWithRolesViewModel model, List<string> roles) {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!result.Succeeded) {
            ViewBag.Error = "Error removing user roles.";
            return View(model);
        }

        result = await _userManager.AddToRolesAsync(user, roles);

        if (!result.Succeeded) {
            ViewBag.Error = "Error adding roles.";
            return View(model);
        }
        ViewBag.Message = "User roles updated successfully!";
        return RedirectToAction("UserManager");
    }

    // POST /Admin/DeleteUser/{id}
    public async Task<IActionResult> DeleteUser(string id) {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded) {
            ViewBag.Message = "User deleted successfully!";
        } else {
            ViewBag.Error = "Error deleting user.";
        }

        return RedirectToAction("UserManager");
    }
}
