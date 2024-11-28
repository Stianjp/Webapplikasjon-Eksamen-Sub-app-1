namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Sub_App_1.Models;

/// <summary>
/// Handles navigation and informational pages of the application.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger"/> to log application events and errors.</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Displays the menu page of the application.
    /// </summary>
    /// <returns>A view representing the menu page.</returns>
    public IActionResult Menu()
    {
        return View();
    }

    /// <summary>
    /// Displays the home page of the application.
    /// </summary>
    /// <returns>A view representing the home page.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the privacy policy page of the application.
    /// </summary>
    /// <returns>A view representing the privacy policy page.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Handles errors and displays an error page with diagnostic information.
    /// </summary>
    /// <remarks>
    /// The error information includes the current request ID for troubleshooting purposes.
    /// </remarks>
    /// <returns>A view representing the error page with diagnostic information.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
