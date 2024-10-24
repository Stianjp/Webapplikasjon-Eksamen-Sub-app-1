namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.Models;

[Authorize(Roles = UserRoles.RegularUser + "," + UserRoles.Administrator)]
public class RegularUserController : Controller {
    public IActionResult Dashboard() {
        return View();
    }
}
