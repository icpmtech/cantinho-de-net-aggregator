namespace MarketAnalyticHub.Controllers
{
  using MarketAnalyticHub.Services;
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;

  [ApiController]
  [Route("api/[controller]")]
  public class SymbolsApiController : ControllerBase
  {
    private readonly ApiService _apiService;

    public SymbolsApiController(ApiService apiService)
    {
      _apiService = apiService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Get(string query)
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        return BadRequest("Keyword cannot be empty");
      }

      var data = await _apiService.GetApiDataAsync(query);
      return Ok(data);
    }
    /// <summary>
    /// Retrieves the current price details for a specified stock symbol.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., AAPL).</param>
    /// <param name="region">Region code (e.g., US).</param>
    /// <returns>JSON object containing stock price details.</returns>
    [HttpGet("stock-price")]
    public async Task<IActionResult> GetStockPrice(
        [FromQuery] string symbol,
        [FromQuery] string region = "US")
    {
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest("Symbol parameter cannot be empty.");
      }

      // Optional: Validate symbol format (e.g., only uppercase letters and numbers)
      if (!System.Text.RegularExpressions.Regex.IsMatch(symbol, @"^[A-Z0-9]+$"))
      {
        return BadRequest("Symbol parameter contains invalid characters.");
      }

      try
      {
        var data = await _apiService.GetStockPriceAsync(symbol, region);
        return Ok(data);
      }
      catch (HttpRequestException ex)
      {
        // Optionally log the exception here if not already logged in the service
        return StatusCode(503, "Error fetching data from Yahoo Finance API.");
      }
      catch (Exception ex)
      {
        // Optionally log the exception here
        return StatusCode(500, "An unexpected error occurred.");
      }
    }
    
    [HttpGet("search-symbol-today")]
    public async Task<IActionResult> GetToday(string query)
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        return BadRequest("Keyword cannot be empty");
      }

      var data = await _apiService.GetApiDataAsync(query);
      return Ok(data);
    }
  }
}
