namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.DAL.Interfaces;
using Sub_App_1.Models;

[Authorize(Roles = UserRoles.Administrator)]
public class AdminController : Controller
{
    private readonly IUserRepository _userRepository;

    public AdminController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET /Admin/UserManager
    public async Task<IActionResult> UserManager()
    {
        var users = await _userRepository.GetAllUsersAsync();
        var userWithRoles = new List<UserWithRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userRepository.GetRolesAsync(user);
            userWithRoles.Add(new UserWithRolesViewModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Roles = roles
            });
        }

        return View(userWithRoles); // pass the list of users with their roles to the view
    }

    // GET /Admin/EditUser/{id}
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new UserWithRolesViewModel
        {
            UserId = user.Id,
            Username = user.UserName,
            Roles = await _userRepository.GetRolesAsync(user)  // fetch current roles
        };

        ViewBag.AllRoles = (await _userRepository.GetAllRolesAsync()).Select(r => r.Name).ToList();  // pass all roles to the view for selection
        return View(model);
    }

    // POST /Admin/EditUser
    [HttpPost]
    public async Task<IActionResult> EditUser(UserWithRolesViewModel model, List<string> roles)
    {
        var user = await _userRepository.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _userRepository.GetRolesAsync(user);
        var result = await _userRepository.RemoveFromRolesAsync(user, currentRoles);

        if (!result.Succeeded)
        {
            ViewBag.Error = "Error removing user roles.";
            return View(model);
        }

        result = await _userRepository.AddToRolesAsync(user, roles);

        if (!result.Succeeded)
        {
            ViewBag.Error = "Error adding roles.";
            return View(model);
        }
        ViewBag.Message = "User roles updated successfully!";
        return RedirectToAction("UserManager");
    }

    // GET /Admin/DeleteUser/{id}
    [HttpGet]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new UserWithRolesViewModel
        {
            UserId = user.Id,
            Username = user.UserName,
            Roles = await _userRepository.GetRolesAsync(user)
        };

        return View(model);
    }

    // POST /Admin/DeleteUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string id)
    {
        if (_userRepository == null)
        {
            throw new Exception("_userRepository is null");
        }

        
        if (string.IsNullOrEmpty(id))
        {
            TempData["Error"] = "Invalid user ID.";
            return RedirectToAction("UserManager");
        }

        var user = await _userRepository.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(); // Returnerer NotFound for ugyldig bruker
        }

        var result = await _userRepository.DeleteUserAsync(user);
        if (result == null || !result.Succeeded)
        {
            TempData["Error"] = result == null ? "An unexpected error occurred." : "Error deleting user.";
            return RedirectToAction("UserManager");
        }

        TempData["Message"] = "User deleted successfully!";
        return RedirectToAction("UserManager");
    }

}