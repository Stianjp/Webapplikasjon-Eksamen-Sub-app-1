namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.Models;

/// <summary>
/// Manages user account-related actions such as login, logout, registration, password changes, and account deletion.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="userManager">Manages user accounts and roles.</param>
    /// <param name="signInManager">Handles user sign-in and sign-out operations.</param>
    /// <param name="roleManager">Provides role management capabilities.</param>
    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Displays the default view for the Account section.
    /// </summary>
    /// <returns>The default view.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Authenticates and logs in a user.
    /// </summary>
    /// <param name="username">The username provided by the user.</param>
    /// <param name="password">The password provided by the user.</param>
    /// <returns>A redirect to another action or the login view with an error message.</returns>
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Perform the sign-in attempt
        var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Get the user and their roles after successful login
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Check if the user has any roles
                if (roles.Any())
                {
                    // redirect to the products page if the user has a role
                    return RedirectToAction("Productsindex", "Products");
                }
                else
                {
                    // redirect to the home page if the user has no roles
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        // If login failed, show an error message
        ViewBag.Error = "Invalid username or password.";
        return View("Index");
    }

    /// <summary>
    /// Logs out the currently signed-in user.
    /// </summary>
    /// <returns>A redirect to the home page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Registers a new user account with the specified credentials and role.
    /// </summary>
    /// <param name="username">The username for the new account.</param>
    /// <param name="password">The password for the new account.</param>
    /// <param name="confirmPassword">The password confirmation.</param>
    /// <param name="role">The role to assign to the user.</param>
    /// <returns>A redirect to the home page or the registration view with validation errors.</returns>
    [HttpPost]
    public async Task<IActionResult> Register(string username, string password, string confirmPassword, string role)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ModelState.AddModelError(string.Empty, "Username, password, and password confirmation cannot be null or empty.");
            return View("Index", ModelState);
        }

        // Prevent the use of the username "Admin" and similar
        var reservedUsernames = new[] { "Admin", "Administrator", "Superuser", "Root", "Default_Producer" }; // reserved usernames
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

        var user = new IdentityUser
        {
            UserName = username
        };
        var result = await _userManager.CreateAsync(user, password); // create user (attempt)

        if (result.Succeeded)
        {
            // Prevent users from assigning themselves the "Administrator" role during registration
            if (role == UserRoles.Administrator)
            {
                await _userManager.DeleteAsync(user); // rollback user creation
                ModelState.AddModelError(string.Empty, "You are not allowed to assign the Administrator role.");
                ViewBag.Error = "Error during registration.";
                return View("Index", ModelState);
            }

            if (string.IsNullOrEmpty(role))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.RegularUser); // default role
            }
            else
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid role specified.");
                    ViewBag.Error = "Error during registration.";
                    return View("Index", ModelState);
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        ViewBag.Error = "Error during registration.";
        return View("Index", ModelState);
    }

    /// <summary>
    /// Displays the change password view.
    /// </summary>
    /// <returns>The view for changing passwords.</returns>
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(); // ChangePassword.cshtml
    }

    /// <summary>
    /// Changes the password of the currently logged-in user.
    /// </summary>
    /// <param name="currentPassword">The current password of the user.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="confirmPassword">The confirmation of the new password.</param>
    /// <returns>A redirect to the account index or the change password view with validation errors.</returns>
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

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index");
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);  // Keeps the user signed in after password change
            return RedirectToAction("Index", "Account");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View();
    }

    /// <summary>
    /// Displays the account deletion confirmation view.
    /// </summary>
    /// <returns>The account deletion confirmation view.</returns>
    [HttpGet]
    public IActionResult DeleteAccount()
    {
        return View(); // DeleteAccount.cshtml
    }

    /// <summary>
    /// Deletes the currently logged-in user's account if the provided password is correct.
    /// </summary>
    /// <param name="password">The password of the user to confirm account deletion.</param>
    /// <returns>A redirect to the home page or the account deletion view with validation errors.</returns>
    [HttpPost]
    public async Task<IActionResult> DeleteAccountConfirmed(string password)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index");
        }

        // Check if the provided password is correct
        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!passwordCheck.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Incorrect password.");
            ViewBag.Error = "Password confirmation failed.";
            return View("DeleteAccount");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View("DeleteAccount");
    }
}
