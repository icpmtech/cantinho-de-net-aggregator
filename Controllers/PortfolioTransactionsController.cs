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
  public class PortfolioTransactionsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly PortfolioService _portfolioService;

    public PortfolioTransactionsController(ApplicationDbContext context, PortfolioService portfolioService)
    {
      _context = context;
      _portfolioService = portfolioService;
    }

    public async Task<IActionResult> PortfolioItemsList()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolioItems = await _context.PortfolioItems
                                          .Where(p => p.UserId == userId)
                                          .ToListAsync();

      return Json(portfolioItems);
    }



    // POST: api/PortfolioTransactions/CreateItem
    [HttpPost("api/PortfolioTransactions/CreateItem")]
    public async Task<IActionResult> CreateItem([FromBody] PortfolioItem portfolioItem)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (portfolioItem == null)
        return BadRequest();

      portfolioItem.UserId = userId;
      _context.PortfolioItems.Add(portfolioItem);
      await _context.SaveChangesAsync();
      return Json(new { success = true, item = portfolioItem });
    }

    // PUT: api/PortfolioTransactions/UpdateItem
    [HttpPut("api/PortfolioTransactions/UpdateItem")]
    public async Task<IActionResult> UpdateItem([FromBody] PortfolioItem portfolioItem)
    {
      if (portfolioItem == null)
        return BadRequest();

      if (!_context.PortfolioItems.Any(p => p.Id == portfolioItem.Id))
        return NotFound();

      try
      {
        _context.Update(portfolioItem);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!PortfolioItemExists(portfolioItem.Id))
          return NotFound();
        else
          throw;
      }
      return Json(new { success = true });
    }

    // DELETE: api/PortfolioTransactions/DeleteItem/5
    [HttpDelete("api/PortfolioTransactions/DeleteItem/{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
      var portfolioItem = await _context.PortfolioItems.FindAsync(id);
      if (portfolioItem == null)
        return NotFound();

      _context.PortfolioItems.Remove(portfolioItem);
      await _context.SaveChangesAsync();
      return Json(new { success = true });
    }

    private bool PortfolioItemExists(int id)
    {
      return _context.PortfolioItems.Any(e => e.Id == id);
    }




  }
}
