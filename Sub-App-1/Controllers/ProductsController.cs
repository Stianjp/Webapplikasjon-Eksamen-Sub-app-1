namespace Sub_App_1.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Sub_App_1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sub_App_1.DAL.Interfaces;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;

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

    private readonly List<string> _availableAllergens = new List<string>{
        "Milk",
        "Egg",
        "Peanut",
        "Soy",
        "Wheat",
        "Tree Nut",
        "Shellfish",
        "Fish",
        "Sesame",
        "None"
    };

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    private bool IsAdmin()
    {
        return User.IsInRole(UserRoles.Administrator);
    }

    // GET: Products (available to all, including not logged in users)
    public async Task<IActionResult> Productsindex()
    {
        var products = await _productRepository.GetAllProductsAsync();
        return View(products);
    }

    // GET: Products/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // GET: Products/Create
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public IActionResult Create()
    {
        ViewBag.AllergenOptions = _availableAllergens;
        ViewBag.CategoryOptions = _availableCategories;
        return View(new Product());
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Create(Product product, List<string> SelectedAllergens)
    {
        try
        {
            // Set ProducerId before ModelState validation
            product.ProducerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(product.ProducerId))
            {
                return BadRequest("Producer ID is invalid.");
            }

            // Remove ModelState errors for Producer and ProducerId
            ModelState.Remove("ProducerId");
            ModelState.Remove("Producer");

            // Handle CategoryList
            if (product.CategoryList == null || !product.CategoryList.Any())
            {
                ModelState.AddModelError("CategoryList", "Please select at least one category.");
            }

            // Save selected allergens as a comma-separated string
            product.Allergens = SelectedAllergens != null ? string.Join(",", SelectedAllergens) : null;

            // Remove 'Allergens' from ModelState since we're setting it manually
            ModelState.Remove("Allergens");

            if (ModelState.IsValid)
            {
                await _productRepository.CreateProductAsync(product);
                return RedirectToAction(nameof(Productsindex));
            }

            // Log ModelState errors
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine("ModelState Error: " + error.ErrorMessage);
                }
            }

            ViewBag.AllergenOptions = _availableAllergens;
            ViewBag.CategoryOptions = _availableCategories;
            return View(product);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");

            ViewBag.AllergenOptions = _availableAllergens;
            ViewBag.CategoryOptions = _availableCategories;
            return View(product);
        }
    }

    // GET: Products/Edit/{id}
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }

        ViewBag.AllergenOptions = _availableAllergens;
        ViewBag.CategoryOptions = _availableCategories;
        return View(product);
    }

    // POST: Products/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Edit(int id, Product updatedProduct, List<string> SelectedAllergens)
    {
        if (id != updatedProduct.Id)
        {
            return BadRequest();
        }

        var product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }

        // Set ProducerId before ModelState validation
        updatedProduct.ProducerId = product.ProducerId;
        ModelState.Remove("ProducerId");
        ModelState.Remove("Producer");

        // Handle CategoryList
        if (updatedProduct.CategoryList == null || !updatedProduct.CategoryList.Any())
        {
            ModelState.AddModelError("CategoryList", "Please select at least one category.");
        }
        else
        {
            // Set Category from CategoryList
            updatedProduct.Category = string.Join(",", updatedProduct.CategoryList);
            ModelState.Remove("Category"); // Remove ModelState error for Category
        }

        // Save selected allergens as a comma-separated string
        updatedProduct.Allergens = SelectedAllergens != null ? string.Join(",", SelectedAllergens) : null;
        ModelState.Remove("Allergens");

        if (ModelState.IsValid)
        {
            try
            {
                // Update product properties
                product.Name = updatedProduct.Name;
                product.Description = updatedProduct.Description;
                product.Category = updatedProduct.Category; // Stored as a string
                product.Calories = updatedProduct.Calories;
                product.Protein = updatedProduct.Protein;
                product.Fat = updatedProduct.Fat;
                product.Carbohydrates = updatedProduct.Carbohydrates;
                product.Allergens = updatedProduct.Allergens;

                await _productRepository.UpdateProductAsync(product);
                return RedirectToAction(nameof(Productsindex));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex is DbUpdateConcurrencyException)
                {
                    if (await _productRepository.GetProductByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        // Log ModelState errors
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine("ModelState Error: " + error.ErrorMessage);
        }

        ViewBag.AllergenOptions = _availableAllergens;
        ViewBag.CategoryOptions = _availableCategories;
        return View(updatedProduct);
    }

    // GET: Products/Delete/{id} (only FoodProducers and Admins can delete products)
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }
        return View(product);
    }

    // POST: Products/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (!IsAdmin() && product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }

        var success = await _productRepository.DeleteProductAsync(id);

        if (!success)
        {
            return BadRequest("Unable to delete product.");
        }

        return RedirectToAction(nameof(Productsindex));
    }

    // Private method to generate category options using TagBuilder
    private string GenerateCategoryOptions(string? selectedCategories)
    {
        var selectedCategoryList = string.IsNullOrEmpty(selectedCategories) ? new List<string>() : selectedCategories.Split(',').ToList();
        var selectList = new TagBuilder("select");

        selectList.Attributes.Add("name", "Category");
        selectList.Attributes.Add("id", "Category");
        selectList.Attributes.Add("class", "form-control");
        selectList.Attributes.Add("multiple", "multiple");
        selectList.Attributes.Add("required", "required");

        foreach (var category in _availableCategories)
        {
            var option = new TagBuilder("option");
            option.Attributes.Add("value", category);

            if (selectedCategoryList.Contains(category))
            {
                option.Attributes.Add("selected", "selected");
            }
            option.InnerHtml.Append(category);
            selectList.InnerHtml.AppendHtml(option);
        }

        var writer = new System.IO.StringWriter();
        selectList.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    // GET: Products/Index with sorting functionality
    public async Task<IActionResult> Index(string sortOrder, string currentSort, string sortDirection)
    {
        ViewData["CurrentSort"] = sortOrder;
        ViewData["CurrentDirection"] = sortDirection == "asc" ? "desc" : "asc";

        ViewData["NameSortParam"] = "Name";
        ViewData["CategorySortParam"] = "Category";
        ViewData["CaloriesSortParam"] = "Calories";
        ViewData["ProteinSortParam"] = "Protein";
        ViewData["FatSortParam"] = "Fat";
        ViewData["CarbohydratesSortParam"] = "Carbohydrates";

        var products = await _productRepository.GetAllProductsAsync();

        // Convert to IQueryable for sorting
        IQueryable<Product> productsQuery = products.AsQueryable();

        switch (sortOrder)
        {
            case "Name":
                productsQuery = sortDirection == "desc" ? productsQuery.OrderByDescending(p => p.Name) : productsQuery.OrderBy(p => p.Name);
                break;
            case "Category":
                productsQuery = sortDirection == "desc" ? productsQuery.OrderByDescending(p => p.Category) : productsQuery.OrderBy(p => p.Category);
                break;
            case "Calories":
                productsQuery = sortDirection == "desc" ? productsQuery.OrderByDescending(p => p.Calories) : productsQuery.OrderBy(p => p.Calories);
                break;
            case "Protein":
                productsQuery = sortDirection == "desc" ? productsQuery.OrderByDescending(p => p.Protein) : productsQuery.OrderBy(p => p.Protein);
                break;
            case "Fat":
                productsQuery = sortDirection == "desc" ? productsQuery.OrderByDescending(p => p.Fat) : productsQuery.OrderBy(p => p.Fat);
                break;
            case "Carbohydrates":
                productsQuery = sortDirection == "desc" ? productsQuery.OrderByDescending(p => p.Carbohydrates) : productsQuery.OrderBy(p => p.Carbohydrates);
                break;
            default:
                productsQuery = productsQuery.OrderBy(p => p.Name);
                break;
        }

        return View("ProductsIndex", productsQuery.ToList());
    }
}