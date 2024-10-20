using Microsoft.AspNetCore.Mvc;  // For Controller og IActionResult
using Microsoft.EntityFrameworkCore;  // For bruk av DbContext
using Sub_App_1.Data;  // For ApplicationDbContext
using Sub_App_1.Models;  // For Product-modellen
using System.Security.Claims;  // For ClaimTypes og User.FindFirstValue


public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Products
    public async Task<IActionResult> Productsindex()
    {
        var products = await _context.Products.ToListAsync();
        return View(products);
    }

    // GET: Products/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,Calories,Protein,Fat,Carbohydrates")] Product product)
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

            return View(product);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return View(product);
        }
    }

    // GET: Products/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // POST: Products/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Calories,Protein,Fat,Carbohydrates")] Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(product);
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
        return View(product);
    }

    // GET: Products/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // POST: Products/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Productsindex));
    }
}
