
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Sub_App_1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sub_App_1.DAL.Interfaces;

namespace Sub_App_1.Controllers
{
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

        // GET: Products/Details/{id} (only FoodProducers and Admins)
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                Console.WriteLine($"Error: Not found");
                return NotFound();
            }
            return View(product);
        }

        // GET: Products/Create (only FoodProducers and Admins can create products)
        [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
        public IActionResult Create()
        {
            ViewBag.AllergenOptions = _availableAllergens;
            ViewBag.CategoryOptions = GenerateCategoryOptions(null);
            return View(new Product());
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
        public async Task<IActionResult> Create([Bind("Name,Description,Category,Calories,Protein,Fat,Carbohydrates,Allergens")] Product product, List<string> SelectedAllergens)
        {
            // debug
            Console.WriteLine("Create POST action invoked");

            try
            {
                // Save selected allergens as a comma-separated string
                product.Allergens = SelectedAllergens != null ? string.Join(",", SelectedAllergens) : null;

                // Remove 'Allergens' from ModelState since we're setting it manually
                ModelState.Remove("Allergens");

                if (ModelState.IsValid)
                {
                    product.ProducerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Log the ProducerId
                    Console.WriteLine($"ProducerId: {product.ProducerId}");

                    if (string.IsNullOrEmpty(product.ProducerId))
                    {
                        return BadRequest("Producer ID is invalid.");
                    }

                    await _productRepository.CreateProductAsync(product);
                    return RedirectToAction(nameof(Productsindex));
                }

                // Regenerate allergen and category options if model state is invalid
                ViewBag.AllergenOptions = _availableAllergens;
                ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                // Regenerate allergen and category options in case of error
                ViewBag.AllergenOptions = _availableAllergens;
                ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
                return View(product);
            }
        }
        // GET: Products/Edit/{id} (only FoodProducers and Admins can edit products)
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

            // Split allergens into a list for checkboxes and send options to the view
            ViewBag.AllergenOptions = _availableAllergens;
            ViewBag.CategoryOptions = GenerateCategoryOptions(product.Category);
            return View(product);
        }

        // POST: Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.FoodProducer + "," + UserRoles.Administrator)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Category,Calories,Protein,Fat,Carbohydrates,Allergens")] Product updatedProduct, List<string> SelectedAllergens)
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

            // Log SelectedAllergens for debugging
            Console.WriteLine("Selected Allergens: " + string.Join(", ", SelectedAllergens));

            // Set the Allergens field manually and clear ModelState errors related to it
            ModelState.Remove("Allergens");
            updatedProduct.Allergens = SelectedAllergens != null ? string.Join(",", SelectedAllergens) : null;

            if (ModelState.IsValid)
            {
                try
                {
                    // Update product properties
                    product.Name = updatedProduct.Name;
                    product.Description = updatedProduct.Description;
                    product.Category = updatedProduct.Category;
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

            // If ModelState is invalid, log errors for debugging
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("ModelState Error: " + error.ErrorMessage);
            }

            // Regenerate options if ModelState is invalid
            ViewBag.AllergenOptions = _availableAllergens;
            ViewBag.CategoryOptions = GenerateCategoryOptions(updatedProduct.Category);
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
}
