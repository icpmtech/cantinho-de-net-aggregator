using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/events")]
  [ApiController]
  public class EventsController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: api/Events
    [HttpGet]
    public ActionResult<IEnumerable<Event>> GetEvents()
    {
     var result=  _context.StockEvents?.Select(s => new Event
      {
        Id = s.Id,
        Title = s.EventName,
        Start = DateTime.Parse(s.Date), // Assuming Date is a string in a format that JavaScript can parse
        End = DateTime.Parse(s.Date),
        Calendar = "Other", // Default category
        Location = s.Source,
        Guests = s.Sentiment, // Mapping Sentiment to Guests for demonstration
        Description = s.Details,
        AllDay = false, // Assuming events are not all-day events by default
        ExtendedProps = new ExtendedProps 
        {
          Impact = s.Impact,
          Price = (decimal)s.Price,
          PriceChange = (decimal)s.PriceChange,
          PortfolioItemId = s.PortfolioItemId
        }
      }).ToList();
      return result;
    }

    // GET: api/Events/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEvent(int id)
    {
      var eventItem = await _context.StockEvents.FindAsync(id);

      if (eventItem == null)
      {
        return NotFound();
      }

      return new Event
      {
        Id = eventItem.Id,
        Title = eventItem.EventName,
        Start = DateTime.Parse(eventItem.Date),
        End = DateTime.Parse(eventItem.Date),
        Calendar = "Other",
        Location = eventItem.Source,
        Guests = eventItem.Sentiment,
        Description = eventItem.Details,
        AllDay = false,
        ExtendedProps = new ExtendedProps
        {
          Impact = eventItem.Impact,
          Price = (decimal)eventItem.Price,
          PriceChange = (decimal)eventItem.PriceChange,
          PortfolioItemId = eventItem.PortfolioItemId
        }
      };
    }

    // PUT: api/Events/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEvent(int id, Event eventItem)
    {
      if (id != eventItem.Id)
      {
        return BadRequest();
      }

      var stockEvent = await _context.StockEvents.FindAsync(id);
      if (stockEvent == null)
      {
        return NotFound();
      }

      stockEvent.EventName = eventItem.Title;
      stockEvent.Date = eventItem.Start.ToString();
      stockEvent.Source = eventItem.Location;
      stockEvent.Sentiment = eventItem.Guests;
      stockEvent.Details = eventItem.Description;
      stockEvent.Impact = eventItem.ExtendedProps?.Impact;
      stockEvent.Price = eventItem.ExtendedProps?.Price;
      stockEvent.PriceChange = eventItem.ExtendedProps?.PriceChange;
      stockEvent.PortfolioItemId = eventItem.ExtendedProps?.PortfolioItemId ?? stockEvent.PortfolioItemId;

      _context.Entry(stockEvent).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!EventExists(id))
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

    // POST: api/Events
    [HttpPost]
    public async Task<ActionResult<Event>> PostEvent(Event eventItem)
    {
      var stockEvent = new StockEvent
      {
        EventName = eventItem.Title,
        Date = eventItem.Start.ToString(),
        Source = eventItem.Location,
        Sentiment = eventItem.Guests,
        Details = eventItem.Description,
        Impact = eventItem.ExtendedProps?.Impact,
        Price = eventItem.ExtendedProps?.Price,
        PriceChange = eventItem.ExtendedProps?.PriceChange,
        PortfolioItemId = eventItem.ExtendedProps?.PortfolioItemId ?? 0
      };

      _context.StockEvents.Add(stockEvent);
      await _context.SaveChangesAsync();

      eventItem.Id = stockEvent.Id;

      return CreatedAtAction("GetEvent", new { id = eventItem.Id }, eventItem);
    }

    // DELETE: api/Events/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
      var eventItem = await _context.StockEvents.FindAsync(id);
      if (eventItem == null)
      {
        return NotFound();
      }

      _context.StockEvents.Remove(eventItem);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool EventExists(int id)
    {
      return _context.StockEvents.Any(e => e.Id == id);
    }
  }
}
