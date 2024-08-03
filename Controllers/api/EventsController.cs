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
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
    {
      var events = await _context.StockEvents.Select(e => new Event
      {
        Id = e.Id,
        Url = e.Url,
        Title = e.Title,
        Start = e.Start,
        End = e.End,
        AllDay = e.AllDay,
        ExtendedProps = new ExtendedProps
        {
          Calendar = e.Calendar,
          Location = e.Location,
          Guests = e.Guests,
          Description = e.Description,
          Impact = e.Impact,
          Sentiment = e.Sentiment,
          Source = e.Source,
          Price = e.Price,
          PriceChange = e.PriceChange,
          PortfolioItemId = e.PortfolioItemId
        }
      }).ToListAsync();

      return events;
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
        Url = eventItem.Url,
        Title = eventItem.Title,
        Start = eventItem.Start,
        End = eventItem.End,
        AllDay = eventItem.AllDay,
        ExtendedProps = new ExtendedProps()
        {
          Calendar = eventItem.Calendar,
          Location = eventItem.Location,
          Guests = eventItem.Guests,
          Description = eventItem.Description,
          Impact = eventItem.Impact,
          Sentiment = eventItem.Sentiment,
          Source = eventItem.Source,
          Price = eventItem.Price,
          PriceChange = eventItem.PriceChange,
          PortfolioItemId = eventItem.PortfolioItemId
        }
      };
    }

    // PUT: api/Events/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEvent(int id, Event eventItem)
    {
      Event eventItem1 = eventItem;
      if (id != eventItem1.Id)
      {
        return BadRequest();
      }

      var stockEvent = await _context.StockEvents.FindAsync(id);
      if (stockEvent == null)
      {
        return NotFound();
      }

      stockEvent.Url = eventItem1.Url;
      stockEvent.Title = eventItem1.Title;
      stockEvent.Start = eventItem1.Start;
      stockEvent.End = eventItem1.End;
      stockEvent.AllDay = eventItem1.AllDay;
      stockEvent.Calendar = eventItem1.ExtendedProps.Calendar;
      stockEvent.Location = eventItem1.ExtendedProps.Location;
      stockEvent.Guests = eventItem1.ExtendedProps.Guests;
      stockEvent.Description = eventItem1.ExtendedProps.Description;
      stockEvent.Impact = eventItem1.ExtendedProps.Impact;
      stockEvent.Sentiment = eventItem1.ExtendedProps.Sentiment;
      stockEvent.Source = eventItem1.ExtendedProps.Source;
      stockEvent.Price = eventItem1.ExtendedProps.Price;
      stockEvent.PriceChange = eventItem1.ExtendedProps.PriceChange;
      stockEvent.PortfolioItemId = eventItem1.ExtendedProps.PortfolioItemId;

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
        Url = eventItem.Url,
        Title = eventItem.Title,
        Start = eventItem.Start,
        End = eventItem.End,
        AllDay = eventItem.AllDay,
        Calendar = eventItem.ExtendedProps.Calendar,
        Location = eventItem.ExtendedProps.Location,
        Guests = eventItem.ExtendedProps.Guests,
        Description = eventItem.ExtendedProps.Description,
        Impact = eventItem.ExtendedProps.Impact,
        Sentiment = eventItem.ExtendedProps.Sentiment,
        Source = eventItem.ExtendedProps.Source,
        Price = eventItem.ExtendedProps.Price,
        PriceChange = eventItem.ExtendedProps.PriceChange,
        PortfolioItemId = eventItem.ExtendedProps.PortfolioItemId
      };

      _context.StockEvents.Add(stockEvent);
      await _context.SaveChangesAsync();

      eventItem.Id = stockEvent.Id; // Update the eventItem with the generated ID

      return CreatedAtAction("GetEvent", new { id = stockEvent.Id }, eventItem);
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
