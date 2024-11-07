namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Mvc;
using Sub_App_1.DAL.Interfaces;
using Sub_App_1.Models;

public class AccountController : Controller
{
    private readonly IUserRepository _userRepository;

    public AccountController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // /Account/Index
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Perform the sign-in attempt
        var result = await _userRepository.LoginAsync(username, password);

        if (result.Succeeded)
        {
            // Get the user and their roles after successful login
            var user = await _userRepository.FindByNameAsync(username);
            if (user != null)
            {
                var roles = await _userRepository.GetRolesAsync(user) ?? new List<string>();

                // Check if the user has any roles
                if (roles.Any())
                {
                    // Redirect to the products page if the user has a role
                    return RedirectToAction("Productsindex", "Products");
                }
                else
                {
                    // Redirect to the home page if the user has no roles
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        // If login failed, show an error message
        ViewBag.Error = "Invalid username or password.";
        return View("Index");
    }

    // /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _userRepository.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    // /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(string username, string password, string confirmPassword, string role)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ModelState.AddModelError(string.Empty, "Username, password, and password confirmation cannot be null or empty.");
            return View("Index", ModelState);
        }

        // Prevent the use of the username "Admin" and similar
        var reservedUsernames = new[] { "Admin", "Administrator", "Superuser", "Root" }; // reserved usernames
        if (reservedUsernames.Contains(username, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "The username is reserved and cannot be used.");
            return View("Index", ModelState);
        }

        if (password != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return View("Index", ModelState);
        }

        var result = await _userRepository.RegisterAsync(username, password); // create user (attempt)

        if (result.Succeeded)
        {
            var user = await _userRepository.FindByNameAsync(username);

            // Prevent users from assigning themselves the "Administrator" role during registration
            if (role == UserRoles.Administrator)
            {
                await _userRepository.DeleteUserAsync(user); // rollback user creation
                ModelState.AddModelError(string.Empty, "You are not allowed to assign the Administrator role.");
                ViewBag.Error = "Error during registration.";
                return View("Index", ModelState);
            }

            if (string.IsNullOrEmpty(role))
            {
                await _userRepository.AddToRoleAsync(user, UserRoles.RegularUser); // default role
            }
            else
            {
                if (await _userRepository.RoleExistsAsync(role))
                {
                    await _userRepository.AddToRoleAsync(user, role);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid role specified.");
                    ViewBag.Error = "Error during registration.";
                    return View("Index", ModelState);
                }
            }

            await _userRepository.LoginAsync(username, password);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        ViewBag.Error = "Error during registration.";
        return View("Index", ModelState);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(); // ChangePassword.cshtml
    }

    // /Account/ChangePassword
    [HttpPost]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            ModelState.AddModelError(string.Empty, "All password fields are required.");
            return View();  // Returns the view so user can correct inputs
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "New password and confirmation password do not match.");
            return View();
        }

        var user = await _userRepository.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index");
        }

        var result = await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            // Keeps the user signed in after password change
            await _userRepository.LoginAsync(user.UserName, newPassword);
            return RedirectToAction("Index", "Account");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View();
    }

    // Show a confirmation view for deleting the account
    [HttpGet]
    public IActionResult DeleteAccount()
    {
        return View(); // DeleteAccount.cshtml
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccountConfirmed(string password)
    {
        var user = await _userRepository.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index");
        }

        // Check if the provided password is correct
        var passwordCheck = await _userRepository.LoginAsync(user.UserName, password);
        if (!passwordCheck.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Incorrect password.");
            ViewBag.Error = "Password confirmation failed.";
            return View("DeleteAccount");
        }

        var result = await _userRepository.DeleteUserAsync(user);
        if (result.Succeeded)
        {
            await _userRepository.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View("DeleteAccount");
    }
}