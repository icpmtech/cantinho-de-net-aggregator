using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  public class DividendController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public DividendController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Dividend>> GetDividend(int id)
    {
      var dividend = await _context.Dividends.FindAsync(id);
      if (dividend == null)
      {
        return NotFound();
      }

      return dividend;
    }

    [HttpPost]
    public async Task<ActionResult<Dividend>> PostDividend(DividendViewModel dividendViewModel)
    {
      if (dividendViewModel == null)
      {
        return BadRequest("Dividend data is null.");
      }

      var portfolioItem = await _context.PortfolioItems.FindAsync(dividendViewModel.PortfolioItemId);
      if (portfolioItem == null)
      {
        return BadRequest("Invalid PortfolioItemId.");
      }

      // Map the ViewModel to the entity
      var dividend = new Dividend
      {
        PortfolioItemId = dividendViewModel.PortfolioItemId,
        Symbol = dividendViewModel.Symbol,
        Amount = dividendViewModel.Amount,
        ExDate = dividendViewModel.ExDate,
        PaymentDate = dividendViewModel.PaymentDate
      };

      _context.Dividends.Add(dividend);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetDividend), new { id = dividend.Id }, dividend);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutDividend(int id, DividendViewModel dividendViewModel)
    {
      if (dividendViewModel == null || id != dividendViewModel.Id) // Ensure the ID matches
      {
        return BadRequest("Invalid data.");
      }

      var dividend = await _context.Dividends.FindAsync(id);
      if (dividend == null)
      {
        return NotFound();
      }

      var portfolioItem = await _context.PortfolioItems.FindAsync(dividendViewModel.PortfolioItemId);
      if (portfolioItem == null)
      {
        return BadRequest("Invalid PortfolioItemId.");
      }

      // Update the entity with the ViewModel data
      dividend.PortfolioItemId = dividendViewModel.PortfolioItemId;
      dividend.Symbol = dividendViewModel.Symbol;
      dividend.Amount = dividendViewModel.Amount;
      dividend.ExDate = dividendViewModel.ExDate;
      dividend.PaymentDate = dividendViewModel.PaymentDate;

      _context.Entry(dividend).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!DividendExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDividend(int id)
    {
      var dividend = await _context.Dividends.FindAsync(id);
      if (dividend == null)
      {
        return NotFound();
      }

      _context.Dividends.Remove(dividend);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool DividendExists(int id)
    {
      return _context.Dividends.Any(e => e.Id == id);
    }

    [HttpGet("ByPortfolioItem/{portfolioItemId}")]
    public async Task<ActionResult<IEnumerable<Dividend>>> GetDividendsByPortfolioItem(int portfolioItemId)
    {
      var dividends = await _context.Dividends
                                    .Where(d => d.PortfolioItemId == portfolioItemId)
                                    .ToListAsync();

      if (dividends == null || dividends.Count == 0)
      {
        return NotFound("No dividends found for the specified portfolio item.");
      }

      return Ok(dividends);
    }
  }
}
