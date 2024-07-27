using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers
{
    public class PortfolioItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PortfolioItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

    // GET: PortfolioItems
    public async Task<IActionResult> Index(string sortOrder, string searchQuery, int? pageNumber)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      ViewData["CurrentSort"] = sortOrder;
      ViewData["OperationTypeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "operationType_desc" : "";
      ViewData["UserIdSortParm"] = sortOrder == "UserId" ? "userId_desc" : "UserId";
      // Add other sort parameters as needed

      var portfolios = from p in _context.PortfolioItems where p.UserId == userId select p;

      if (!String.IsNullOrEmpty(searchQuery))
      {
        portfolios = portfolios.Where(p => p.Symbol.Contains(searchQuery)
                                   || p.OperationType.Contains(searchQuery));
      }

      switch (sortOrder)
      {
        case "operationType_desc":
          portfolios = portfolios.OrderByDescending(p => p.OperationType);
          break;
        case "UserId":
          portfolios = portfolios.OrderBy(p => p.UserId);
          break;
        case "userId_desc":
          portfolios = portfolios.OrderByDescending(p => p.UserId);
          break;
        // Add other cases for sorting as needed
        default:
          portfolios = portfolios.OrderBy(p => p.OperationType);
          break;
      }

      int pageSize = 10;
      return View(await PaginatedList<PortfolioItem>.CreateAsync(portfolios.AsNoTracking(), pageNumber ?? 1, pageSize));
    }
    

        // GET: PortfolioItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioItem = await _context.PortfolioItems
                .Include(p => p.Industry)
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolioItem == null)
            {
                return NotFound();
            }

            return View(portfolioItem);
        }

        // GET: PortfolioItems/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id");
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id");
            return View();
        }

        // POST: PortfolioItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OperationType,UserId,PortfolioId,Symbol,PurchaseDate,Quantity,PurchasePrice,CurrentPrice,Commission,CompanyId")] PortfolioItem portfolioItem)
        {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (ModelState.IsValid)
            {
               portfolioItem.UserId = userId;
                _context.Add(portfolioItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", portfolioItem.CompanyId);
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", portfolioItem.PortfolioId);
            return View(portfolioItem);
        }

        // GET: PortfolioItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioItem = await _context.PortfolioItems.FindAsync(id);
            if (portfolioItem == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", portfolioItem.CompanyId);
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", portfolioItem.PortfolioId);
            return View(portfolioItem);
        }

        // POST: PortfolioItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OperationType,UserId,PortfolioId,Symbol,PurchaseDate,Quantity,PurchasePrice,CurrentPrice,Commission,CompanyId")] PortfolioItem portfolioItem)
        {
            if (id != portfolioItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
          _context.Update(portfolioItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PortfolioItemExists(portfolioItem.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", portfolioItem.CompanyId);
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", portfolioItem.PortfolioId);
            return View(portfolioItem);
        }

        // GET: PortfolioItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioItem = await _context.PortfolioItems
                .Include(p => p.Industry)
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolioItem == null)
            {
                return NotFound();
            }

            return View(portfolioItem);
        }

        // POST: PortfolioItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolioItem = await _context.PortfolioItems.FindAsync(id);
            if (portfolioItem != null)
            {
                _context.PortfolioItems.Remove(portfolioItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioItemExists(int id)
        {
            return _context.PortfolioItems.Any(e => e.Id == id);
        }
    }
}
