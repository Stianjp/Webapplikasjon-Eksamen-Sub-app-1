using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Diagnostics;
using Sub_App_1.Models; 

namespace Sub_App_1.Controllers
{
   public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            var roles = User.IsInRole("FoodProducer") ? "FoodProducer" :
                        User.IsInRole("Admin") ? "Admin" :
                        "RegularUser";

            if (roles == "FoodProducer")
            {
                return RedirectToAction("ProducerDashboard", "FoodProducer");
            }
            else if (roles == "Admin")
            {
                // Kommentert ut for nå, men legg til logikk for Admin dashboard senere
                // return RedirectToAction("Dashboard", "Admin");
                return Content("Admin dashboard kommer snart!");
            }
            else if (roles == "RegularUser")
            {
                // Kommentert ut for nå, men legg til logikk for RegularUser dashboard senere
                // return RedirectToAction("Dashboard", "RegularUser");
                return Content("RegularUser dashboard kommer snart!");
            }
        }

        // Hvis brukeren ikke er logget inn, vis en generell startside
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

}
