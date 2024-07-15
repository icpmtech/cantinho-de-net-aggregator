using AspnetCoreMvcFull.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AspnetCoreMvcFull.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public partial class QualitativeEventsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public QualitativeEventsController(ApplicationDbContext context)
    {
      _context = context;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QualitativeEvent>>> GetQualitativeEvents()
    {
      return await _context.QualitativeEvents
                           .Include(q => q.News)
                           .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QualitativeEvent>> GetQualitativeEvent(int id)
    {
      var qualitativeEvent = await _context.QualitativeEvents
                                           .Include(q => q.News)
                                           .FirstOrDefaultAsync(q => q.Id == id);

      if (qualitativeEvent == null)
      {
        return NotFound();
      }

      return qualitativeEvent;
    }

    [HttpPost]
    public async Task<ActionResult<QualitativeEvent>> PostQualitativeEvent(QualitativeEventDto qualitativeEventDto)
    {
      var newsItems = await _context.News
                                    .Where(n => qualitativeEventDto.NewsIds.Contains((int)n.Id))
                                    .ToListAsync();

      var qualitativeEvent = new QualitativeEvent
      {
        Symbol = qualitativeEventDto.Symbol,
        EventDescription = qualitativeEventDto.EventDescription,
        EventDate = qualitativeEventDto.EventDate,
        News = newsItems
      };

      _context.QualitativeEvents.Add(qualitativeEvent);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetQualitativeEvent", new { id = qualitativeEvent.Id }, qualitativeEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutQualitativeEvent(int id, QualitativeEventDto qualitativeEventDto)
    {
      var qualitativeEvent = await _context.QualitativeEvents
                                           .Include(q => q.News)
                                           .FirstOrDefaultAsync(q => q.Id == id);

      if (qualitativeEvent == null)
      {
        return NotFound();
      }

      var newsItems = await _context.News
                                    .Where(n => qualitativeEventDto.NewsIds.Contains((int)n.Id))
                                    .ToListAsync();

      qualitativeEvent.Symbol = qualitativeEventDto.Symbol;
      qualitativeEvent.EventDescription = qualitativeEventDto.EventDescription;
      qualitativeEvent.EventDate = qualitativeEventDto.EventDate;
      qualitativeEvent.News = newsItems;

      _context.Entry(qualitativeEvent).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!QualitativeEventExists(id))
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
    public async Task<IActionResult> DeleteQualitativeEvent(int id)
    {
      var qualitativeEvent = await _context.QualitativeEvents.FindAsync(id);
      if (qualitativeEvent == null)
      {
        return NotFound();
      }

      _context.QualitativeEvents.Remove(qualitativeEvent);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool QualitativeEventExists(int id)
    {
      return _context.QualitativeEvents.Any(e => e.Id == id);
    }
  }
  }
