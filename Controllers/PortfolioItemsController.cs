using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models;
using System.Security.Claims;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Controllers.api;

namespace MarketAnalyticHub.Controllers
{
  public class PortfolioItemsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly PortfolioService _portfolioService;

    public PortfolioItemsController(ApplicationDbContext context, PortfolioService portfolioService)
    {
      _context = context;
      _portfolioService = portfolioService;
    }

    // GET: PortfolioItems
    public async Task<IActionResult> Index(string sortOrder, string searchQuery, int? pageNumber, int pageSize = 10, string tab = "list")
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      ViewData["CurrentSort"] = sortOrder;
      ViewData["CurrentFilter"] = searchQuery;
      ViewData["CurrentPageSize"] = pageSize;
      ViewData["CurrentTab"] = tab;

      ViewData["OperationTypeSortParm"] = String.IsNullOrEmpty(sortOrder) ? "operationType_desc" : "";
      ViewData["UserIdSortParm"] = sortOrder == "UserId" ? "userId_desc" : "UserId";

      var portfolios = from p in _context.PortfolioItems.Include(p => p.Industry).Include(p => p.Portfolio)
                       where p.UserId == userId
                       select p;

      if (!String.IsNullOrEmpty(searchQuery))
      {
        portfolios = portfolios.Where(p => p.Symbol.Contains(searchQuery)
                               || p.OperationType.Contains(searchQuery));
      }

      portfolios = sortOrder switch
      {
        "operationType_desc" => portfolios.OrderByDescending(p => p.OperationType),
        "UserId" => portfolios.OrderBy(p => p.UserId),
        "userId_desc" => portfolios.OrderByDescending(p => p.UserId),
        _ => portfolios.OrderBy(p => p.OperationType),
      };

      var portfolioItems = await portfolios.AsNoTracking().ToListAsync();
      var overallStats = await _portfolioService.GetTotalPortfolioOverall(userId);

      var portfolioStatistics = portfolioItems.Select(p => new PortfolioStatistic
      {

        TotalInvestment = (decimal)overallStats.TotalMarketValue,
        CurrentMarketValue = (decimal)overallStats.TotalCustMarketValue,
        TotalDifferenceValue = (decimal)overallStats.TotalDifferenceValue,
        TotalDividends = (decimal)overallStats.TotalDividends,
        TotalProfit = (decimal)overallStats.TotalPortfolioProfit,
        TotalDifferencePercentage = (decimal)overallStats.TotalDifferencePercentage,
        TotalProfitDifferencePercentage = (decimal)overallStats.TotalDifferenceWithDividendsPercentage,
      });

      var groupedPortfolios = portfolioItems
                              .GroupBy(p => p.Symbol)
                              .Select(g => new GroupedPortfolioItems
                              {
                                TotalInvestment = g.Sum(p => p.TotalInvestment),
                                CurrentMarketValue = g.Sum(p => p.CurrentMarketValue),
                                Symbol = g.Key,
                                Items = g.ToList()
                              });

      var model = PaginatedList<GroupedPortfolioItems>.Create(groupedPortfolios.AsQueryable(), pageNumber ?? 1, pageSize);

      return View(model);
    }




    // GET: PortfolioItems/PortfolioItems/Details/5
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
      var dataPrice = await _portfolioService.GetCurrentPriceAsync(portfolioItem.Symbol);
      portfolioItem.CurrentPrice = (decimal)dataPrice.CurrentPrice;
      return View(portfolioItem);
    }

    // GET: PortfolioItems/Create
    public IActionResult Create()
    {
      ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
      ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Name");
      ViewData["IndustryList"] = new SelectList(_context.Companies, "Id", "Industry");
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
      ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", portfolioItem.CompanyId);
      ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Name", portfolioItem.PortfolioId);
      return View(portfolioItem);
    }

    // GET: PortfolioItems/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var portfolioItem = await _context.PortfolioItems
       .Include(p => p.Industry) // Include Industry navigation property
       .FirstOrDefaultAsync(p => p.Id == id);
      if (portfolioItem == null)
      {
        return NotFound();
      }

     

      var dataPrice = await _portfolioService.GetCurrentPriceAsync(portfolioItem.Symbol);
      portfolioItem.CurrentPrice = (decimal)dataPrice.CurrentPrice;
      ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", portfolioItem.CompanyId);
      ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Name", portfolioItem.PortfolioId);
      ViewData["IndustryList"] = new SelectList(_context.Companies, "Id", "Industry", portfolioItem.CompanyId);
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
      ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", portfolioItem.CompanyId);
      ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Name", portfolioItem.PortfolioId);
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

    [HttpGet("dividends")]
    public async Task<IActionResult> Calendar(string symbol)
    {
      return View();
    }

    private bool PortfolioItemExists(int id)
    {
      return _context.PortfolioItems.Any(e => e.Id == id);
    }
  }
}
