using AspnetCoreMvcFull.Models.Portfolio;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  public class PortfolioItemController : Controller
  {
    private readonly ApplicationDbContext _context;

    public PortfolioItemController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: api/PortfolioItem/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PortfolioItem>> GetPortfolioItem(int id)
    {
      var portfolioItem = await _context.PortfolioItems.FindAsync(id);

      if (portfolioItem == null)
      {
        return NotFound();
      }

      return portfolioItem;
    }

    // POST: api/PortfolioItem
    [HttpPost]
    public async Task<ActionResult<PortfolioItem>> PostPortfolioItem(PortfolioItem portfolioItem)
    {
      _context.PortfolioItems.Add(portfolioItem);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetPortfolioItem", new { id = portfolioItem.Id }, portfolioItem);
    }

    // PUT: api/PortfolioItem/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPortfolioItem(int id, PortfolioItem portfolioItem)
    {
      if (id != portfolioItem.Id)
      {
        return BadRequest();
      }

      _context.Entry(portfolioItem).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!PortfolioItemExists(id))
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

    // DELETE: api/PortfolioItem/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePortfolioItem(int id)
    {
      var portfolioItem = await _context.PortfolioItems.FindAsync(id);
      if (portfolioItem == null)
      {
        return NotFound();
      }

      _context.PortfolioItems.Remove(portfolioItem);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool PortfolioItemExists(int id)
    {
      return _context.PortfolioItems.Any(e => e.Id == id);
    }
  }
}
