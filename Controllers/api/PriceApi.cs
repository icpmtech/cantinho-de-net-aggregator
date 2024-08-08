namespace MarketAnalyticHub.Controllers
{
  using MarketAnalyticHub.Models.SetupDb;
  using MarketAnalyticHub.Services;
  using MarketAnalyticHub.Services.ApiDataApp.Services;
  using Microsoft.AspNetCore.Mvc;
  using System;
  using System.Linq;
  using System.Threading.Tasks;

  [ApiController]
  [Route("api/[controller]")]
  public class PriceApiController : ControllerBase
  {
    private readonly ISymbolService _symbolService;
    private readonly ApplicationDbContext _context;
    private readonly IYahooFinanceService _yahooFinanceService;

    public PriceApiController(ApplicationDbContext context, ISymbolService symbolService, IYahooFinanceService yahooFinanceService)
    {
      _symbolService = symbolService;
      _yahooFinanceService = yahooFinanceService;
      _context = context;
    }

    [HttpGet("getCurrentPrice")]
    public async Task<IActionResult> GetCurrentPrice(string? symbol, string? dateValue)
    {
      if (string.IsNullOrEmpty(symbol))
      {
        return BadRequest("Symbol cannot be null or empty.");
      }

      if (string.IsNullOrEmpty(dateValue))
      {
        return BadRequest("Date value cannot be null or empty.");
      }

      if (!DateTime.TryParse(dateValue, out DateTime parsedDate))
      {
        return BadRequest("Invalid date format.");
      }

      var currentStockData = await _yahooFinanceService.GetDailyHistoricalDataAsync(symbol, parsedDate);
      if (currentStockData == null)
      {
        return NotFound($"No data found for symbol {symbol} on date {dateValue}.");
      }

      var symbolData = new
      {
        symbol = symbol,
        currentStockData = currentStockData.FirstOrDefault()
      };

      return Ok(symbolData);
    }


    [HttpGet("getSymbolsCurrentData")]
    public async Task<IActionResult> GetSymbolsCurrentData(DateTime dateValue)
    {
      var symbols = await _symbolService.GetSymbolsAsync();
      var symbolsData = symbols.Select(async s => new
      {
        symbol = s,
        currentStockData = await _yahooFinanceService.GetDailyHistoricalDataAsync(s, dateValue)
      }).Select(t => t.Result);

      return Ok(symbolsData);
    }
  }
}
