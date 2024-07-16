using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers.api
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
    public async Task<ActionResult<Dividend>> PostDividend(Dividend dividend)
    {
      _context.Dividends.Add(dividend);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetDividend), new { id = dividend.Id }, dividend);
    }

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
