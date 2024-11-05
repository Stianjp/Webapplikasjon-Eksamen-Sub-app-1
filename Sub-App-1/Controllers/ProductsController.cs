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
    public IActionResult Create() {
        // Generate category options
        ViewBag.CategoryOptions = GenerateCategoryOptions(null);
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Create([Bind("Name,Description,Category,Calories,Protein,Fat,Carbohydrates")] Product product) {
        try {
            if (ModelState.IsValid) {
                product.ProducerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Productsindex));
            }
            // If model state is invalid, regenerate category options
            ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
            return View(product);
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
            // In case of error, regenerate category options
            ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
            return View(product);
        }
    }

    // GET: Products/Edit/{id} (only FoodProducers and Admins can edit products)
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Edit(int id) {
        var product = await _context.Products.FindAsync(id);

        if (product == null) {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) {
            return Forbid(); // Return 403 Forbidden instead of NotFound
        }

        // Generate category options with the selected categories
        ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
        return View(product);
    }

    // POST: Products/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Category,Calories,Protein,Fat,Carbohydrates")] Product updatedProduct) {
        if (id != updatedProduct.Id) {
            return BadRequest();
        }
        var product = await _context.Products.FindAsync(id);

        if (product == null) {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) {
            return Forbid();
        }

        if (ModelState.IsValid) {
            try {
                // Update the product properties
                product.Name = updatedProduct.Name;
                product.Description = updatedProduct.Description;
                product.Category = updatedProduct.Category;
                product.Calories = updatedProduct.Calories;
                product.Protein = updatedProduct.Protein;
                product.Fat = updatedProduct.Fat;
                product.Carbohydrates = updatedProduct.Carbohydrates;
                product.ImageUrl = updatedProduct.ImageUrl;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Productsindex));
            } catch (DbUpdateConcurrencyException) {
                if (!_context.Products.Any(p => p.Id == id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
        }
        // If model state is invalid, regenerate category options
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
}
