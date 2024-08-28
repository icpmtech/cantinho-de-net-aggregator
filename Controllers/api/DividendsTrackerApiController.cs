using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class DividendsTrackerApiController : ControllerBase
  {
    private readonly IDividendsTrackerRepository _dividendsTrackerRepository;
    private readonly IIndicesDividendsTrackerRepository _indicesDividendsTrackerRepository;
    private readonly ApplicationDbContext _context;
    public DividendsTrackerApiController(ApplicationDbContext context,IDividendsTrackerRepository dividendsTrackerRepository, IIndicesDividendsTrackerRepository indicesDividendsTrackerRepository)
    {
      _context = context;
      _dividendsTrackerRepository = dividendsTrackerRepository;
      _indicesDividendsTrackerRepository = indicesDividendsTrackerRepository;
    }
    [HttpGet("dividends")]
    public async Task<ActionResult<IEnumerable<DividendsTracker>>> GetDividends(string region, string index)
    {
      var dividends = await _dividendsTrackerRepository.GetDividendsAsync(region, index);

      if (!dividends.Any())
      {
        return NotFound($"No dividends found for region: {region} with index: {index}");
      }

      return Ok(dividends);
    }

    // POST: api/DividendsTracker
    [HttpPost("add-dividend")]
    public async Task<ActionResult<DividendsTracker>> AddDividend([FromBody] DividendsTrackerDto dividendDto)
    {
      if (dividendDto == null || !ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var dividend = new DividendsTracker
      {
        Company = dividendDto.Company,
        Ticker = dividendDto.Ticker,
        Country = dividendDto.Country,
        Region = dividendDto.Region,
        Exchange = dividendDto.Exchange,
        SharePrice = dividendDto.SharePrice,
        PrevDividend = dividendDto.PrevDividend,
        PayDateDividend = dividendDto.PayDateDividend,
         ExDateDividend = dividendDto.ExDateDividend
      };

      foreach (var indexId in dividendDto.SelectedIndices)
      {
        var index = await _indicesDividendsTrackerRepository.FindAsync(indexId);
        if (index != null)
        {
          dividend.DividendIndices.Add(new DividendIndex
          {
            DividendsTracker = dividend,
            IndexDividendsTracker = index
          });
        }
      }

      _dividendsTrackerRepository.AddDividendAsync(dividend);
    

      return CreatedAtAction(nameof(GetDividendById), new { id = dividend.Id }, dividend);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetDividendById(int id)
    {
      if (id <= 0)
      {
        return BadRequest("Invalid ID.");
      }

      var dividend = await _dividendsTrackerRepository.GetDividendByIdAsync(id);

      if (dividend == null)
      {
        return NotFound();
      }

      return Ok(dividend);
    }

    [HttpPut("update-dividend")]
    public async Task<IActionResult> UpdateDividend([FromBody] DividendsTracker dividendsTracker)
    {
      if (dividendsTracker == null || !ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var existingDividend = await _dividendsTrackerRepository.GetDividendByIdAsync(dividendsTracker.Id);
      if (existingDividend == null)
      {
        return NotFound();
      }

      try
      {
        // Update the properties of the existing entity
        existingDividend.Company = dividendsTracker.Company;
        existingDividend.Ticker = dividendsTracker.Ticker;
        existingDividend.Country = dividendsTracker.Country;
        existingDividend.Region = dividendsTracker.Region;
        existingDividend.Exchange = dividendsTracker.Exchange;
        existingDividend.SharePrice = dividendsTracker.SharePrice;
        existingDividend.PrevDividend = dividendsTracker.PrevDividend;
        existingDividend.ExDateDividend = dividendsTracker.ExDateDividend;
        existingDividend.PayDateDividend = dividendsTracker.PayDateDividend;

        await _dividendsTrackerRepository.UpdateDividendAsync(existingDividend);
      }
      catch (Exception ex)
      {
        // Log the exception (ex) as needed
        return StatusCode(StatusCodes.Status500InternalServerError, "Error updating data.");
      }

      return Ok();
    }




    [HttpGet("all-dividends")]
    public async Task<ActionResult<IEnumerable<DividendsTracker>>> GetAllDividends()
    {
      var dividends = await _dividendsTrackerRepository.GetAllDividendsAsync();

      if (!dividends.Any())
      {
        return NotFound($"No dividends found.");
      }

      return Ok(dividends);
    }

    [HttpGet("indices")]
    public async Task<ActionResult<IEnumerable<string>>> GetIndices(string region)
    {
      var indices = await _indicesDividendsTrackerRepository.GetIndicesAsync(region);

      if (indices == null || !indices.Any())
      {
        return NotFound($"No indices found for region: {region}");
      }

      return Ok(indices);
    }
  }
}
