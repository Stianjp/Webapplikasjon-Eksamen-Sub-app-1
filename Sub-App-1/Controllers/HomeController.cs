namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Sub_App_1.Models;
using Microsoft.AspNetCore.Identity;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) {
        _logger = logger;
    }

    public IActionResult Index() {
        if (User.Identity != null && User.Identity.IsAuthenticated) {
            // Redirect users based on their roles
            if (User.IsInRole(UserRoles.FoodProducer)) {
                return RedirectToAction("Dashboard", "FoodProducer");
            } else if (User.IsInRole(UserRoles.Administrator)) {
                return RedirectToAction("Dashboard", "FoodProducer"); // can be the same as food producer
            } else if (User.IsInRole(UserRoles.RegularUser)) {
                return RedirectToAction("Dashboard", "RegularUser");
            }
        }
        // General home page for users who are not logged in
        return View();
    }

    public IActionResult Privacy() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}