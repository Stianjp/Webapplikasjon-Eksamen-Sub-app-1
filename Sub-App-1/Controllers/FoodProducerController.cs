using Microsoft.AspNetCore.Mvc;  // For Controller og IActionResult
using Microsoft.EntityFrameworkCore;  // For bruk av DbContext
using Sub_App_1.Data;  // For ApplicationDbContext
using Sub_App_1.Models;  // For Product-modellen
using System.Security.Claims;  // For ClaimTypes og User.FindFirstValue
using Microsoft.AspNetCore.Authorization;

/*@* TODO 
*Implementer GetProducerData() for å hente produkter
*Dobbel sjekk av model sine attributter passer med visningen i view dashboard cards
* Omdiriger til ProductsController sin Create-aksjon til FoodProducerController sin lag nytt produkt
for å gjennombruk db og metoder, da de skal gjøre det samme både i produktsiden og dashboard. 
*Metoderw vil da være create, edit og delete et Product.
 *@ */

[Authorize(Roles = "FoodProducer")]
public class FoodProducerController : Controller
{
    public IActionResult ProducerDashboard()
    {
        /*var model = GetProducerData();*/ //Denne metoden må lages
        return View("ProducerDashboard"); // Assuming this view is located at Views/FoodProducer/ProducerDashboard.cshtml
    }
}