using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sub_App_1.Models;

namespace Sub_App_1.Controllers;

public class ProductsController : Controller {
    public IActionResult Productsindex() {
        return View();
    }
}