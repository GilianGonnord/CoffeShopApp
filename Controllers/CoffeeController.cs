using CoffeeShopApp.Data;
using CoffeeShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApp.Controllers;

[Authorize]
public class CoffeeController : Controller
{
    private readonly CoffeeShopContext _context;

    public CoffeeController(CoffeeShopContext context)
    {
        _context = context;
    }

    // GET: Coffee
    public async Task<IActionResult> Index()
    {
        if (!HasClaim(CoffeeClaims.CanViewCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var coffees = await _context.Coffees.ToListAsync();
        return View(coffees);
    }

    // GET: Coffee/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!HasClaim(CoffeeClaims.CanViewCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (id == null) return NotFound();

        var coffee = await _context.Coffees.FirstOrDefaultAsync(m => m.Id == id);
        if (coffee == null) return NotFound();

        return View(coffee);
    }

    // GET: Coffee/Create
    public IActionResult Create()
    {
        if (!HasClaim(CoffeeClaims.CanManageCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return View();
    }

    // POST: Coffee/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,Price,Origin,RoastLevel,IsAvailable")] Coffee coffee)
    {
        if (!HasClaim(CoffeeClaims.CanManageCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            _context.Add(coffee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(coffee);
    }

    // GET: Coffee/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (!HasClaim(CoffeeClaims.CanManageCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (id == null) return NotFound();

        var coffee = await _context.Coffees.FindAsync(id);
        if (coffee == null) return NotFound();

        return View(coffee);
    }

    // POST: Coffee/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Origin,RoastLevel,IsAvailable,CreatedAt")] Coffee coffee)
    {
        if (!HasClaim(CoffeeClaims.CanManageCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (id != coffee.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(coffee);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoffeeExists(coffee.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(coffee);
    }

    // GET: Coffee/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (!HasClaim(CoffeeClaims.CanManageCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (id == null) return NotFound();

        var coffee = await _context.Coffees.FirstOrDefaultAsync(m => m.Id == id);
        if (coffee == null) return NotFound();

        return View(coffee);
    }

    // POST: Coffee/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!HasClaim(CoffeeClaims.CanManageCoffee))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var coffee = await _context.Coffees.FindAsync(id);
        if (coffee != null)
        {
            _context.Coffees.Remove(coffee);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CoffeeExists(int id)
    {
        return _context.Coffees.Any(e => e.Id == id);
    }

    private bool HasClaim(string claimType)
    {
        return User.HasClaim(claimType, "true");
    }
}