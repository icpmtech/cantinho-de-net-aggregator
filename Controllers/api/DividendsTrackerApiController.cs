using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Mvc;
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

    public DividendsTrackerApiController(IDividendsTrackerRepository dividendsTrackerRepository, IIndicesDividendsTrackerRepository indicesDividendsTrackerRepository)
    {
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
