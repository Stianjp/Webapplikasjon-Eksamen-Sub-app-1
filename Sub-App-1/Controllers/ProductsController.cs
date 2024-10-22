using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sub_App_1.Data;
using Sub_App_1.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[Authorize]  // ensure only logged-in users can access products
public class ProductsController : Controller {
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context) {
        _context = context;
    }

    // GET: Products (available to all logged-in users)
    [Authorize(Roles = UserRoles.RegularUser + "," + UserRoles.FoodProducer)]  // Both FoodProducer and RegularUser can view products
    public async Task<IActionResult> Productsindex() {
        var products = await _context.Products.ToListAsync();
        return View(products);
    }

    // GET: Products/Create (only FoodProducers can create products)
    [Authorize(Roles = UserRoles.FoodProducer)]
    public IActionResult Create() {
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer)]  // Only FoodProducers can post products
    public async Task<IActionResult> Create([Bind("Name,Description,Category,Calories,Protein,Fat,Carbohydrates")] Product product) {
        try {
            if (ModelState.IsValid) {
                product.ProducerId = User.FindFirstValue(ClaimTypes.NameIdentifier);  
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Productsindex));
            }
            return View(product);
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
            return View(product);
        }
    }

    // GET: Products/Edit/{id}
    [Authorize(Roles = UserRoles.FoodProducer)]  // Only FoodProducers can edit products
    public async Task<IActionResult> Edit(int id) {
        var product = await _context.Products.FindAsync(id);
        if (product == null || product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) { // Ensure producer owns the product
            return NotFound();
        }
        return View(product);
    }

    // POST: Products/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Category,Calories,Protein,Fat,Carbohydrates")] Product product) {
        if (id != product.Id || product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) { // Ensure producer owns the product
            return BadRequest();
        }

        if (ModelState.IsValid) {
            try {
                _context.Update(product);
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
        return View(product);
    }

    // GET: Products/Delete/{id}
    [Authorize(Roles = UserRoles.FoodProducer)]  // Only FoodProducers can delete products
    public async Task<IActionResult> Delete(int id) {
        var product = await _context.Products.FindAsync(id);
        if (product == null || product.ProducerId != User.FindFirstValue(ClaimTypes.NameIdentifier)) { // Ensure producer owns the product
            return NotFound();
        }
        return View(product);
    }

    // POST: Products/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.FoodProducer)]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        var product = await _context.Products.FindAsync(id);
        if (product != null && product.ProducerId == User.FindFirstValue(ClaimTypes.NameIdentifier)) { // Ensure producer owns the product
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Productsindex));
    }
}
