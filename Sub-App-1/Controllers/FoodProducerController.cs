namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.Models;

[Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
public class FoodProducerController : Controller {
    // GET: FoodProducer/Dashboard
    public IActionResult Dashboard() {
        return View();
    }
}
