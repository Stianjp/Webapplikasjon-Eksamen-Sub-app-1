namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sub_App_1.Data;
using Sub_App_1.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class ProductsController : Controller {
    private readonly ApplicationDbContext _context; 

    // Define the list of available categories
    private readonly List<string> _availableCategories = new List<string> {
        "Meat",
        "Fish",
        "Vegetable",
        "Fruit",
        "Pasta",
        "Legume",
        "Drink"
    };
    private readonly List<string> _availabeAllergens = new List<string>{
        "Milk",
        "Egg",
        "Peanut",
        "Soy",
        "Wheat",
        "Tree Nut",
        "Shellfish",
        "Fish",
        "Sesame"
    };

    public ProductsController(ApplicationDbContext context) {
        _context = context;
    }

    private bool IsAdmin() {
        return User.IsInRole(UserRoles.Administrator);
    }

    // GET: Products (available to all, including not logged in users)
    public async Task<IActionResult> Productsindex() {
        var products = await _context.Products.ToListAsync();
        return View(products);
    }

    // GET: Products/Details/{id} (only FoodProducers and Admins)
    public async Task<IActionResult> Details(int id) {
        var product = await _context.Products.FindAsync(id);

        if (product == null) {
            Console.WriteLine($"Error: Not found");
            return NotFound();
        }
        return View(product);
    }

    // GET: Products/Create (only FoodProducers and Admins can create products)
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public IActionResult Create()
    {
        ViewBag.AllergenOptions = _availabeAllergens;
        ViewBag.CategoryOptions = GenerateCategoryOptions(null);
        return View();
    }


    // POST: Products/Create
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
public async Task<IActionResult> Create([Bind("Name,Description,Category,Calories,Protein,Fat,Carbohydrates,Allergens")] Product product)
{
    try
    {
        if (ModelState.IsValid)
        {
            product.ProducerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Productsindex));
        }
        // Regenerer allergen- og kategori-alternativer hvis model state er ugyldig
        ViewBag.AllergenOptions = _availabeAllergens;
        ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
        return View(product);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        // Regenerer allergen- og kategori-alternativer i tilfelle feil
        ViewBag.AllergenOptions = _availabeAllergens;
        ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
        return View(product);
    }
}

   // GET: Products/Edit/{id} (only FoodProducers and Admins can edit products)
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid(); // Return 403 Forbidden instead of NotFound
        }

        // Send allergener og kategori-alternativer til viewet
        ViewBag.AllergenOptions = _availabeAllergens;
        ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
        return View(product);
    }


    // POST: Products/Edit/{id}
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Category,Calories,Protein,Fat,Carbohydrates,Allergens")] Product updatedProduct)
{
    if (id != updatedProduct.Id)
    {
        return BadRequest();
    }
    var product = await _context.Products.FindAsync(id);

    if (product == null)
    {
        return NotFound();
    }

    if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
    {
        return Forbid();
    }

    if (ModelState.IsValid)
    {
        try
        {
            // Oppdater produktets egenskaper
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Category = updatedProduct.Category;
            product.Calories = updatedProduct.Calories;
            product.Protein = updatedProduct.Protein;
            product.Fat = updatedProduct.Fat;
            product.Carbohydrates = updatedProduct.Carbohydrates;
            product.Allergens = updatedProduct.Allergens;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Productsindex));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(p => p.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
    }
    // Regenerer allergen- og kategori-alternativer hvis model state er ugyldig
    ViewBag.AllergenOptions = _availabeAllergens;
    ViewBag.CategoryOptions = GenerateCategoryOptions(updatedProduct.Category);
    return View(updatedProduct);
}


    // GET: Products/Delete/{id} (only FoodProducers and Admins can delete products)
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Delete(int id) {
        var product = await _context.Products.FindAsync(id);

        if (product == null) {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) {
            return Forbid();
        }
        return View(product);
    }

    // POST: Products/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        var product = await _context.Products.FindAsync(id);

        if (product == null) {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) {
            return Forbid();
        }
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Productsindex));
    }

    // Private method to generate category options using TagBuilder
    private string GenerateCategoryOptions(string selectedCategories) {
        // Parse the selected categories into a list
        var selectedCategoryList = string.IsNullOrEmpty(selectedCategories) ? new List<string>() : selectedCategories.Split(',').ToList();
        var selectList = new TagBuilder("select");

        selectList.Attributes.Add("name", "Category");
        selectList.Attributes.Add("id", "Category");
        selectList.Attributes.Add("class", "form-control");
        selectList.Attributes.Add("multiple", "multiple");
        selectList.Attributes.Add("required", "required");

        foreach (var category in _availableCategories) {
            var option = new TagBuilder("option");
            option.Attributes.Add("value", category);

            if (selectedCategoryList.Contains(category)) {
                option.Attributes.Add("selected", "selected");
            }
            option.InnerHtml.Append(category);
            selectList.InnerHtml.AppendHtml(option);
        }
        // Render the select list to a string
        var writer = new System.IO.StringWriter();
        selectList.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

public IActionResult Index(string sortOrder, string currentSort, string sortDirection)
{
    // Holder styr på gjeldende sorteringskolonne og retning
    ViewData["CurrentSort"] = sortOrder;
    ViewData["CurrentDirection"] = sortDirection == "asc" ? "desc" : "asc";

    // Sorteringsalternativer for kolonner
    ViewData["NameSortParam"] = "Name";
    ViewData["CategorySortParam"] = "Category";
    ViewData["CaloriesSortParam"] = "Calories";
    ViewData["ProteinSortParam"] = "Protein";
    ViewData["FatSortParam"] = "Fat";
    ViewData["CarbohydratesSortParam"] = "Carbohydrates";

    // Hent produktene
    var products = from p in _context.Products
                   select p;

    // Sorter basert på sortOrder og sortDirection
    switch (sortOrder)
    {
        case "Name":
            products = sortDirection == "desc" ? products.OrderByDescending(p => p.Name) : products.OrderBy(p => p.Name);
            break;
        case "Category":
            products = sortDirection == "desc" ? products.OrderByDescending(p => p.Category) : products.OrderBy(p => p.Category);
            break;
        case "Calories":
            products = sortDirection == "desc" ? products.OrderByDescending(p => p.Calories) : products.OrderBy(p => p.Calories);
            break;
        case "Protein":
            products = sortDirection == "desc" ? products.OrderByDescending(p => p.Protein) : products.OrderBy(p => p.Protein);
            break;
        case "Fat":
            products = sortDirection == "desc" ? products.OrderByDescending(p => p.Fat) : products.OrderBy(p => p.Fat);
            break;
        case "Carbohydrates":
            products = sortDirection == "desc" ? products.OrderByDescending(p => p.Carbohydrates) : products.OrderBy(p => p.Carbohydrates);
            break;
        default:
            products = products.OrderBy(p => p.Name); // Standard sortering
            break;
    }

    return View("ProductsIndex", products.ToList());
}

}
