using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

public class DashboardModel : PageModel
{
    public List<Product> Products { get; set; }

    public void OnGet()
    {
        // Hent siste produkter fra databasen eller en mock-liste
        Products = new List<Product>
        {
            new Product { Name = "Melk", Volume = "1 liter", Category = "Meieriprodukter" },
            new Product { Name = "Brød", Volume = "500 g", Category = "Bakst" },
            new Product { Name = "Smør", Volume = "250 g", Category = "Meieriprodukter" }
        };
    }
}

public class Product
{
    public string Name { get; set; }
    public string Volume { get; set; }
    public string Category { get; set; }
}