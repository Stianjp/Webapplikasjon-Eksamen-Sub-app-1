namespace Sub_App_1.Controllers;

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.Data;
using Sub_App_1.Models;

/*
* TODO: Consider using ASP.NET Core Identity instead.
* Oppdatere Index til Accountindex på grunn av rename av cshtml filen 
*/ 
public class AccountController : Controller {
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(ApplicationDbContext context) {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    // /Account/Index
    public IActionResult Accountindex() {
        return View();
    }

    // /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password) {
        // Search for the user in the database by username (Asynchronous)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        // return an error if no user is found in the db.
        if (user == null) {
            ViewBag.Error = "Invalid username or password.";
            return View("Index");
        }

        // verify the password using password hasher
        var pwd_verification_result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        
        if (pwd_verification_result != PasswordVerificationResult.Success) {
            ViewBag.Error = "Invalid username or password.";
            return View("Index");
        }
        await SignInUser(user);

        // redirect to the dashboard after successful login, or whatever the frontend wants to happen =D
        // TODO !!
        return RedirectToAction("Index", "Home");
    }

    // /Account/Logout
    public async Task<IActionResult> Logout() {
        // Sign out the user
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // Redirect to the home page
        return RedirectToAction("Index", "Home");
    }

    // /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(string username, string password, string accountType) {
        // Check if the username already exists in the database
        if (await _context.Users.AnyAsync(u => u.Username == username)) {
            ViewBag.Error = "Username already exists.";
            return View("Index");
        }

        // Create a new User object with the provided information
        var user = new User {
            Username = username,
            AccountType = Enum.Parse<AccountType>(accountType),
        };
        // have to set the password after creating the user object, as we need the user object.
        user.Password = _passwordHasher.HashPassword(user, password);

        // Add the new user to the database context
        _context.Users.Add(user);
        // Save changes to the database asynchronously
        await _context.SaveChangesAsync();
        // Sign in the registered user
        await SignInUser(user);

        // Todo: proper redirection and stuff.
        return RedirectToAction("Index", "Home");
    }

    // Helper method for signing in a user.
    private async Task SignInUser(User user) {
        // Create claims for the user; these are pieces of user information like account type.
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.AccountType.ToString())
        };
        // Create a ClaimsIdentity, which represents the user's identity
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // Sign in the user by creating a new ClaimsPrincipal and calling SignInAsync
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    }

    // /Account/BrowseAsGuest
    public IActionResult BrowseAsGuest() {
        // todo ..
        // continue browsing without an account
        return RedirectToAction("Index", "Home");
    }
}