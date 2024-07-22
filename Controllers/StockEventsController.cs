using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using System.Security.Claims;

namespace AspnetCoreMvcFull.Controllers
{
    public class StockEventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockEventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StockEvents
        public async Task<IActionResult> Index()
        {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var applicationDbContext = _context.StockEvents
                                       .Include(s => s.PortfolioItem)
                                       .Where(s => s.PortfolioItem.Portfolio.UserId == userId); // Adjust this if UserId is directly in PortfolioItem

      
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: StockEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockEvent = await _context.StockEvents
                .Include(s => s.PortfolioItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stockEvent == null)
            {
                return NotFound();
            }

            return View(stockEvent);
        }

        // GET: StockEvents/Create
        public IActionResult Create()
        {
            ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems, "Id", "Symbol");
            return View();
        }

        // POST: StockEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,EventName,Details,Impact,Sentiment,Source,Price,PriceChange,PortfolioItemId")] StockEvent stockEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stockEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
      ViewBag.PortfolioItemId = new SelectList(_context.PortfolioItems, "Id", "Symbol", stockEvent.PortfolioItemId);

      return View(stockEvent);
        }

        // GET: StockEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockEvent = await _context.StockEvents.FindAsync(id);
            if (stockEvent == null)
            {
                return NotFound();
            }
            ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems, "Id", "Id", stockEvent.PortfolioItemId);
            return View(stockEvent);
        }

        // POST: StockEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,EventName,Details,Impact,Sentiment,Source,Price,PriceChange,PortfolioItemId")] StockEvent stockEvent)
        {
            if (id != stockEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockEventExists(stockEvent.Id))
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
            ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems, "Id", "Id", stockEvent.PortfolioItemId);
            return View(stockEvent);
        }

        // GET: StockEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockEvent = await _context.StockEvents
                .Include(s => s.PortfolioItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stockEvent == null)
            {
                return NotFound();
            }

            return View(stockEvent);
        }

        // POST: StockEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockEvent = await _context.StockEvents.FindAsync(id);
            if (stockEvent != null)
            {
                _context.StockEvents.Remove(stockEvent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockEventExists(int id)
        {
            return _context.StockEvents.Any(e => e.Id == id);
        }
    }
}
