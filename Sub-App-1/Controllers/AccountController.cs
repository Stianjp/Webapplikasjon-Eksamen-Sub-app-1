namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.Models;

public class AccountController : Controller {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // /Account/Index
    public IActionResult Index() {
        return View();
    }

    // /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password) {
        var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded) {
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Error = "Invalid username or password.";
        return View("Index");
    }

    // /Account/Logout
    public async Task<IActionResult> Logout() {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(string username, string password, AccountType accountType) {
        if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
            ModelState.AddModelError(string.Empty, "Username and password cannot be null or empty.");
            return View("Index", ModelState);
        }

        var user = new User {
            UserName = username,
            AccountType = accountType
        };
        var result = await _userManager.CreateAsync(user, password); // create user (attempt)

        if (result.Succeeded) {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");   
        }

        foreach (var error in result.Errors) {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        ViewBag.Error = "Error during registration.";
        return View("Index", ModelState);
    }

    // /Account/BrowseAsGuest
    public IActionResult BrowseAsGuest() {
        // todo ..
        // continue browsing without an account
        return RedirectToAction("Index", "Home");
    }
}