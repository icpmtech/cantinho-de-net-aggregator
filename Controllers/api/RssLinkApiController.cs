using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  public class RSSLinksController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public RSSLinksController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RSSLink>>> GetRSSLinks()
    {
      return await _context.RSSLinks.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RSSLink>> GetRSSLink(int id)
    {
      var rssLink = await _context.RSSLinks.FindAsync(id);

      if (rssLink == null)
      {
        return NotFound();
      }

      return rssLink;
    }

    [HttpPost]
    public async Task<ActionResult<RSSLink>> CreateRSSLink(RSSLink rssLink)
    {
      _context.RSSLinks.Add(rssLink);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetRSSLink), new { id = rssLink.Id }, rssLink);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRSSLink(int id, RSSLink rssLink)
    {
      if (id != rssLink.Id)
      {
        return BadRequest();
      }

      _context.Entry(rssLink).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!RSSLinkExists(id))
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
    public async Task<IActionResult> DeleteRSSLink(int id)
    {
      var rssLink = await _context.RSSLinks.FindAsync(id);
      if (rssLink == null)
      {
        return NotFound();
      }

      _context.RSSLinks.Remove(rssLink);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool RSSLinkExists(int id)
    {
      return _context.RSSLinks.Any(e => e.Id == id);
    }
  }

}
