using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;
using MarketAnalyticHub.Models.SetupDb;

namespace MarketAnalyticHub.Areas.Admin.Controllers
{
  [Route("api/admin/[controller]")]
  [ApiController]
  [Area("Admin")]
  [Authorize]
  public class AdminDividendsTrackerApiController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public AdminDividendsTrackerApiController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: api/admin/DividendsTracker
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DividendsTrackerDto>>> GetDividends()
    {
      var dividends = await _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .ThenInclude(di => di.IndexDividendsTracker)
          .Select(d => new DividendsTrackerDto
          {
            Id = d.Id,
            Company = d.Company,
            Ticker = d.Ticker,
            Country = d.Country,
            Region = d.Region,
            Exchange = d.Exchange,
            SharePrice = d.SharePrice,
            PrevDividend = d.PrevDividend,
            ExDateDividend = d.ExDateDividend,
            PayDateDividend = d.PayDateDividend,
            Indices = d.DividendIndices.Select(di => new IndexDto
            {
              Id = di.IndexDividendsTrackerId,
              Region = di.IndexDividendsTracker.Region
            }).ToList()
          })
          .ToListAsync();

      return dividends;
    }

    // GET: api/admin/DividendsTracker/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DividendsTrackerDto>> GetDividend(int id)
    {
      var dividend = await _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .ThenInclude(di => di.IndexDividendsTracker)
          .FirstOrDefaultAsync(m => m.Id == id);

      if (dividend == null)
      {
        return NotFound();
      }

      var dividendDto = new DividendsTrackerDto
      {
        Id = dividend.Id,
        Company = dividend.Company,
        Ticker = dividend.Ticker,
        Country = dividend.Country,
        Region = dividend.Region,
        Exchange = dividend.Exchange,
        SharePrice = dividend.SharePrice,
        PrevDividend = dividend.PrevDividend,
        ExDateDividend = dividend.ExDateDividend,
        PayDateDividend = dividend.PayDateDividend,
        Indices = dividend.DividendIndices.Select(di => new IndexDto
        {
          Id = di.IndexDividendsTrackerId,
          Region = di.IndexDividendsTracker.Region
        }).ToList()
      };

      return dividendDto;
    }

    // GET: api/admin/DividendsTracker/indices
    [HttpGet("indices")]
    public async Task<ActionResult<IEnumerable<IndexDto>>> GetIndices()
    {
      var indices = await _context.IndexDividendsTrackers
          .Select(i => new IndexDto
          {
            Id = i.Id,
            Region = i.Region
          })
          .ToListAsync();

      return indices;
    }

    // POST: api/admin/DividendsTracker
    [HttpPost]
    public async Task<ActionResult<DividendsTrackerDto>> CreateDividend(DividendsTrackerCreateDto dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var dividend = new DividendsTracker
      {
        Company = dto.Company,
        Ticker = dto.Ticker,
        Country = dto.Country,
        Region = dto.Region,
        Exchange = dto.Exchange,
        SharePrice = dto.SharePrice,
        PrevDividend = dto.PrevDividend,
        ExDateDividend = dto.ExDateDividend,
        PayDateDividend = dto.PayDateDividend,
        DividendIndices = new List<DividendIndex>()
      };

      // Add selected indices
      if (dto.SelectedIndices != null && dto.SelectedIndices.Any())
      {
        foreach (var indexId in dto.SelectedIndices)
        {
          var index = await _context.IndexDividendsTrackers.FindAsync(indexId);
          if (index != null)
          {
            dividend.DividendIndices.Add(new DividendIndex
            {
              DividendsTracker = dividend,
              IndexDividendsTracker = index
            });
          }
        }
      }

      _context.DividendsTrackers.Add(dividend);
      await _context.SaveChangesAsync();

      return CreatedAtAction(
          nameof(GetDividend),
          new { id = dividend.Id },
          new DividendsTrackerDto
          {
            Id = dividend.Id,
            Company = dividend.Company,
            Ticker = dividend.Ticker,
            Country = dividend.Country,
            Region = dividend.Region,
            Exchange = dividend.Exchange,
            SharePrice = dividend.SharePrice,
            PrevDividend = dividend.PrevDividend,
            ExDateDividend = dividend.ExDateDividend,
            PayDateDividend = dividend.PayDateDividend,
            Indices = dividend.DividendIndices.Select(di => new IndexDto
            {
              Id = di.IndexDividendsTrackerId,
              Region = di.IndexDividendsTracker?.Region
            }).ToList()
          });
    }

    // PUT: api/admin/DividendsTracker/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDividend(int id, DividendsTrackerUpdateDto dto)
    {
      if (id != dto.Id)
      {
        return BadRequest();
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        // Find the existing DividendsTracker entity
        var dividend = await _context.DividendsTrackers
            .Include(d => d.DividendIndices)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dividend == null)
        {
          return NotFound();
        }

        // Update properties
        dividend.Company = dto.Company;
        dividend.Ticker = dto.Ticker;
        dividend.Country = dto.Country;
        dividend.Region = dto.Region;
        dividend.Exchange = dto.Exchange;
        dividend.SharePrice = dto.SharePrice;
        dividend.PrevDividend = dto.PrevDividend;
        dividend.ExDateDividend = dto.ExDateDividend;
        dividend.PayDateDividend = dto.PayDateDividend;

        // Handle the selected indices
        var existingIndices = _context.DividendIndices
            .Where(di => di.DividendsTrackerId == id)
            .ToList();

        // Remove unselected indices
        foreach (var existingIndex in existingIndices)
        {
          if (!dto.SelectedIndices.Contains(existingIndex.IndexDividendsTrackerId))
          {
            _context.DividendIndices.Remove(existingIndex);
          }
        }

        // Add new selected indices
        foreach (var indexId in dto.SelectedIndices)
        {
          if (!existingIndices.Any(ei => ei.IndexDividendsTrackerId == indexId))
          {
            var index = await _context.IndexDividendsTrackers.FindAsync(indexId);
            if (index != null)
            {
              dividend.DividendIndices.Add(new DividendIndex
              {
                DividendsTrackerId = dividend.Id,
                IndexDividendsTrackerId = indexId
              });
            }
          }
        }

        await _context.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!DividendsTrackerExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }
    }

    // DELETE: api/admin/DividendsTracker/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDividend(int id)
    {
      var dividend = await _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .FirstOrDefaultAsync(m => m.Id == id);

      if (dividend == null)
      {
        return NotFound();
      }

      _context.DividendsTrackers.Remove(dividend);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool DividendsTrackerExists(int id)
    {
      return _context.DividendsTrackers.Any(e => e.Id == id);
    }
  }

  // DTOs (Data Transfer Objects)
  public class DividendsTrackerDto
  {
    public int Id { get; set; }
    public string Company { get; set; }
    public string Ticker { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string Exchange { get; set; }
    public string SharePrice { get; set; }
    public string PrevDividend { get; set; }
    public DateTime? ExDateDividend { get; set; }
    public DateTime? PayDateDividend { get; set; }
    public List<IndexDto> Indices { get; set; } = new List<IndexDto>();
  }

  public class DividendsTrackerCreateDto
  {
    public string Company { get; set; }
    public string Ticker { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string Exchange { get; set; }
    public string SharePrice { get; set; }
    public string PrevDividend { get; set; }
    public DateTime ExDateDividend { get; set; }
    public DateTime PayDateDividend { get; set; }
    public int[] SelectedIndices { get; set; } = Array.Empty<int>();
  }

  public class DividendsTrackerUpdateDto
  {
    public int Id { get; set; }
    public string Company { get; set; }
    public string Ticker { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string Exchange { get; set; }
    public string SharePrice { get; set; }
    public string PrevDividend { get; set; }
    public DateTime ExDateDividend { get; set; }
    public DateTime PayDateDividend { get; set; }
    public int[] SelectedIndices { get; set; } = Array.Empty<int>();
  }

  public class IndexDto
  {
    public int Id { get; set; }
    public string Region { get; set; }
  }
}
