namespace Sub_App_1.Controllers;

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.Data;
using Sub_App_1.Models;

public class AccountController : Controller {
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context) {
        _context = context;
    }

    // /Account/Index
    public IActionResult Index() {
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

        // TODO: proper password hashing and verification
        if (user.Password != password) {
            ViewBag.Error = "Invalid username or password.";
            return View("Index");
        }

        // User is valid, prepare to sign them in
        // Create claims for the user; these are pieces of user information like account type.
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.AccountType.ToString())
        };
        // Create a ClaimsIdentity, which represents the user's identity
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // Sign in the user by creating a new ClaimsPrincipal and calling SignInAsync
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        // redirect to the dashboard after successful login
        // TODO !!
        return RedirectToAction("Index", "Home");
    }

    // Logout action
    public async Task<IActionResult> Logout() {
        // Sign out the user
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // Redirect to the home page
        return RedirectToAction("Index", "Home");
    }

    // /Account/Register
    [HttpPost]
    public IActionResult Register(string username, string password, string accountType) {
        if (_context.Users.Any(u => u.Username == username)) {
            ViewBag.Error = "Username already exists.";
            return View("Index");
        }

        var user = new User {
            Username = username,
            Password = password,  // TODO: HASHING !!
            AccountType = Enum.Parse<AccountType>(accountType)
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    public IActionResult BrowseAsGuest() {
        // todo ..
        // continue browsing without an account
        return RedirectToAction("Home", "Index");
    }
}