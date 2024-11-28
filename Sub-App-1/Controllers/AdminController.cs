namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sub_App_1.Models;

/// <summary>
/// Provides administrative functionalities such as user management and role assignment.
/// </summary>
[Authorize(Roles = UserRoles.Administrator)]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="userManager">The user manager for managing users.</param>
    /// <param name="roleManager">The role manager for managing roles.</param>
    public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Displays the User Manager view, which lists all users along with their roles.
    /// </summary>
    /// <returns>A view displaying a list of users and their roles.</returns>
    public async Task<IActionResult> UserManager()
    {
        var users = _userManager.Users.ToList();
        var userWithRoles = new List<UserWithRolesViewModel>();

        foreach (var user in users)
        {
            if (user.UserName != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userWithRoles.Add(new UserWithRolesViewModel
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Roles = roles
                });
            }
        }

        return View(userWithRoles);
    }

    /// <summary>
    /// Displays the view to edit a user's roles.
    /// </summary>
    /// <param name="id">The unique identifier of the user to edit.</param>
    /// <returns>A view with the user's details and roles, or a NotFound result if the user does not exist.</returns>
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        string userName = user.UserName ?? string.Empty;
        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserWithRolesViewModel
        {
            UserId = user.Id,
            Username = userName,
            Roles = roles.ToList()
        };

        ViewBag.AllRoles = _roleManager.Roles
            .Where(role => role.Name != null)
            .Select(role => role.Name)
            .ToList();

        return View(model);
    }

    /// <summary>
    /// Updates the roles assigned to a user.
    /// </summary>
    /// <param name="model">The user's information and roles to update.</param>
    /// <param name="roles">A list of roles to assign to the user.</param>
    /// <returns>A redirect to the UserManager view, or the edit user view with errors if the update fails.</returns>
    [HttpPost]
    public async Task<IActionResult> EditUser(UserWithRolesViewModel model, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!result.Succeeded)
        {
            ViewBag.Error = "Error removing user roles.";
            return View(model);
        }

        result = await _userManager.AddToRolesAsync(user, roles);

        if (!result.Succeeded)
        {
            ViewBag.Error = "Error adding roles.";
            return View(model);
        }

        ViewBag.Message = "User roles updated successfully!";
        return RedirectToAction("UserManager");
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>A redirect to the UserManager view, or a NotFound result if the user does not exist.</returns>
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            ViewBag.Message = "User deleted successfully!";
        }
        else
        {
            ViewBag.Error = "Error deleting user.";
        }

        return RedirectToAction("UserManager");
    }

    /// <summary>
    /// Confirms the deletion of a user.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>A redirect to the UserManager view, with success or error messages.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["Error"] = "Invalid user ID.";
            return RedirectToAction("UserManager");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result == null || !result.Succeeded)
        {
            TempData["Error"] = result == null ? "An unexpected error occurred." : "Error deleting user.";
            return RedirectToAction("UserManager");
        }

        TempData["Message"] = "User deleted successfully!";
        return RedirectToAction("UserManager");
    }
}
