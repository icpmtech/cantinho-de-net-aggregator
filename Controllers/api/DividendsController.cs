using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketAnalyticHub.Models.SetupDb;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class DividendsController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public DividendsController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: api/Dividends
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dividend>>> GetDividends()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      return await _context.Dividends
          .Where(p => p.PortfolioItem != null && p.PortfolioItem.UserId == userId)
          .ToListAsync();
    }

    // GET: api/Dividends/5
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

    // POST: api/Dividends
    [HttpPost]
    public async Task<ActionResult<Dividend>> PostDividend(Dividend dividend)
    {
      _context.Dividends.Add(dividend);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetDividend), new { id = dividend.Id }, dividend);
    }

    // PUT: api/Dividends/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDividend(int id, Dividend dividend)
    {
      if (id != dividend.Id)
      {
        return BadRequest();
      }

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

    // DELETE: api/Dividends/5
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
  }
}
