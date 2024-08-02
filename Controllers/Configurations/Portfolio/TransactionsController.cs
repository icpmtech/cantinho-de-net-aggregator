using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketAnalyticHub.Models.Portfolio.Entities;

namespace MarketAnalyticHub.Controllers.Configurations.Portfolio
{
  [Authorize]
  public class TransactionsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public TransactionsController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: Transactions
    public async Task<IActionResult> Index()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      
      var applicationDbContext = _context.Transactions.Include(t => t.PortfolioItem)
        .Where(s => s.PortfolioItem.Portfolio.UserId == userId); ;
      return View(await applicationDbContext.ToListAsync());
    }

    // GET: Transactions/PortfolioItems/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var transaction = await _context.Transactions
          .Include(t => t.PortfolioItem)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (transaction == null)
      {
        return NotFound();
      }

      return View(transaction);
    }

    // GET: Transactions/Create
    public IActionResult Create()
    {
      ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems, "Id", "Id");
      return View();
    }

    // POST: Transactions/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,PortfolioItemId,Quantity,SellPrice,SellDate,Commission,Revenue")] Transaction transaction)
    {
      if (ModelState.IsValid)
      {
        _context.Add(transaction);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems, "Id", "Id", transaction.PortfolioItemId);
      return View(transaction);
    }

    // GET: Transactions/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var transaction = await _context.Transactions.FindAsync(id);
      if (transaction == null)
      {
        return NotFound();
      }
      ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems, "Id", "Id", transaction.PortfolioItemId);
      return View(transaction);
    }

    // POST: Transactions/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PortfolioItemId,Quantity,SellPrice,SellDate,Commission,Revenue")] Transaction transaction)
    {
      if (id != transaction.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(transaction);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!TransactionExists(transaction.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction(nameof(Index));
      }
      ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems, "Id", "Id", transaction.PortfolioItemId);
      return View(transaction);
    }

    // GET: Transactions/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var transaction = await _context.Transactions
          .Include(t => t.PortfolioItem)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (transaction == null)
      {
        return NotFound();
      }

      return View(transaction);
    }

    // POST: Transactions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var transaction = await _context.Transactions.FindAsync(id);
      if (transaction != null)
      {
        _context.Transactions.Remove(transaction);
      }

      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    private bool TransactionExists(int id)
    {
      return _context.Transactions.Any(e => e.Id == id);
    }
  }
}
