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
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Microsoft.Graph;
using MarketAnalyticHub.Models.Portfolio;

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
    public async Task<IActionResult> Index(
    string sortOrder,
    string EventName,
    string Date,
    string Impact,
    string Sentiment,
    string Source,
    string PriceRange,
    int? pageNumber,
    int pageSize = 10,
    string tab = "list")
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var applicationDbContext = _context.StockEvents
                                        .Include(s => s.PortfolioItem)
                                        .Where(s => s.PortfolioItem.Portfolio.UserId == userId);

      ViewData["CurrentSort"] = sortOrder;
      ViewData["CurrentEventName"] = EventName;
      ViewData["CurrentDate"] = Date;
      ViewData["CurrentImpact"] = Impact;
      ViewData["CurrentSentiment"] = Sentiment;
      ViewData["CurrentSource"] = Source;
      ViewData["CurrentPriceRange"] = PriceRange;
      ViewData["CurrentPageSize"] = pageSize;
      ViewData["CurrentTab"] = tab;

      ViewData["OperationTypeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "operationType_desc" : "";
      ViewData["UserIdSortParm"] = sortOrder == "UserId" ? "userId_desc" : "UserId";

      // Filtering logic
      if (!String.IsNullOrEmpty(EventName))
      {
        applicationDbContext = applicationDbContext.Where(s => s.EventName.Contains(EventName));
      }

      if (!String.IsNullOrEmpty(Date))
      {
        DateTime eventDate;
        if (DateTime.TryParse(Date, out eventDate))
        {
          applicationDbContext = applicationDbContext.Where(s => s.Date == eventDate);
        }
      }

      if (!String.IsNullOrEmpty(Impact))
      {
        applicationDbContext = applicationDbContext.Where(s => s.Impact == Impact);
      }

      if (!String.IsNullOrEmpty(Sentiment))
      {
        applicationDbContext = applicationDbContext.Where(s => s.Sentiment == Sentiment);
      }

      if (!String.IsNullOrEmpty(Source))
      {
        applicationDbContext = applicationDbContext.Where(s => s.Source.Contains(Source));
      }

      if (!String.IsNullOrEmpty(PriceRange))
      {
        // Assume PriceRange is in format "min-max"
        var priceParts = PriceRange.Split('-');
        if (priceParts.Length == 2)
        {
          decimal minPrice, maxPrice;
          if (decimal.TryParse(priceParts[0], out minPrice) && decimal.TryParse(priceParts[1], out maxPrice))
          {
            applicationDbContext = applicationDbContext.Where(s => s.Price >= minPrice && s.Price <= maxPrice);
          }
        }
      }

      // Sorting logic
      switch (sortOrder)
      {
        case "operationType_desc":
          applicationDbContext = applicationDbContext.OrderByDescending(s => s.Date);
          break;
        case "UserId":
          applicationDbContext = applicationDbContext.OrderBy(s => s.PortfolioItem.Portfolio.UserId);
          break;
        case "userId_desc":
          applicationDbContext = applicationDbContext.OrderByDescending(s => s.PortfolioItem.Portfolio.UserId);
          break;
        default:
          applicationDbContext = applicationDbContext.OrderBy(s => s.Date);
          break;
      }

      // Pagination logic
      var pageIndex = pageNumber ?? 1;
      var model = await PaginatedList<StockEvent>.CreateAsync(applicationDbContext.AsNoTracking(), pageIndex, pageSize);

      return View(model);
    }

    // GET: StockEvents/PortfolioItems/Details/5
    public async Task<IActionResult> Details(int? id)
        {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems.Where(s => s.Portfolio.UserId == userId) , "Id", "Symbol");
            return View();
        }

        // POST: StockEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,EventName,Details,Impact,Sentiment,Source,Price,PriceChange,PortfolioItemId")] StockEvent stockEvent)
        {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (ModelState.IsValid)
            {
                _context.Add(stockEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

      ViewBag.PortfolioItemId = new SelectList(_context.PortfolioItems.Where(s => s.Portfolio.UserId == userId), "Id", "Symbol", stockEvent.PortfolioItemId);

      return View(stockEvent);
        }

        // GET: StockEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (id == null)
            {
                return NotFound();
            }

            var stockEvent = await _context.StockEvents.FindAsync(id);
            if (stockEvent == null)
            {
                return NotFound();
            }
            ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems.Where(s => s.Portfolio.UserId == userId), "Id", "Symbol", stockEvent.PortfolioItemId);
            return View(stockEvent);
        }

        // POST: StockEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,EventName,Details,Impact,Sentiment,Source,Price,PriceChange,PortfolioItemId")] StockEvent stockEvent)
        {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            ViewData["PortfolioItemId"] = new SelectList(_context.PortfolioItems.Where(s => s.Portfolio.UserId == userId), "Id", "Symbol", stockEvent.PortfolioItemId);
            return View(stockEvent);
        }

        // GET: StockEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
