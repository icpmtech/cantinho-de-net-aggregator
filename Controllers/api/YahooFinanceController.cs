using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  public class YahooFinanceController : ControllerBase
  {
    // GET: api/yahoofinance/price/AAPL
    [HttpGet("price/{symbol}")]
    public async Task<IActionResult> GetPrice(string symbol)
    {
      try
      {
        var price = await YahooService.GetCurrentPriceAsync(symbol);
        return Ok(price);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Message = ex.Message });
      }
    }

    // GET: api/yahoofinance/balance-sheet/AAPL
    [HttpGet("balance-sheet/{symbol}")]
    public async Task<IActionResult> GetBalanceSheet(string symbol)
    {
      try
      {
        var balanceSheet = await YahooService.GetBalanceSheetAsync(symbol);
        return Ok(balanceSheet);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Message = ex.Message });
      }
    }
    // GET: api/yahoofinance/search/{query}
    [HttpGet("search/{query}")]
    public async Task<IActionResult> SearchSymbols(string query)
    {
      try
      {
        var symbols = await YahooService.SearchSymbolsAsync(query);
        return Ok(symbols);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Message = ex.Message });
      }
    }
    // GET: api/yahoofinance/cash-flow/AAPL
    [HttpGet("cash-flow/{symbol}")]
    public async Task<IActionResult> GetCashFlow(string symbol)
    {
      try
      {
        var cashFlow = await YahooService.GetCashFlowAsync(symbol);
        return Ok(cashFlow);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Message = ex.Message });
      }
    }

    // GET: api/yahoofinance/multiple-quotes?symbols=AAPL,MSFT,GOOGL
    [HttpGet("multiple-quotes")]
    public async Task<IActionResult> GetMultipleQuotes([FromQuery] List<string> symbols)
    {
      try
      {
        var quotes = await YahooService.GetMultipleQuotesAsync(symbols);
        return Ok(quotes);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Message = ex.Message });
      }
    }
    public class HistoricalQuoteViewModel
    {
      public DateTime Timestamp { get; set; }
      public decimal Open { get; set; }
      public decimal Close { get; set; }
      public decimal High { get; set; }
      public decimal Low { get; set; }
      public long Volume { get; set; }
    }

    // GET: api/yahoofinance/chart/{symbol}
    [HttpGet("chart/{symbol}")]
    public async Task<IActionResult> GetChart(string symbol, DateTime? startDate = null, DateTime? endDate = null)
    {
      try
      {
        // Use the provided startDate and endDate, or default to one year range ending today
        var finalEndDate = endDate ?? DateTime.Now;
        var finalStartDate = startDate ?? finalEndDate.AddYears(-1);

        // Call the service to get historical data
        var quotes = await YahooService.GetHistoricalDataAsync(symbol, finalStartDate, finalEndDate);

        // Transform dynamic data into strongly typed ViewModel objects
        var result = quotes.Select(q => new HistoricalQuoteViewModel
        {
          Timestamp = q.Timestamp,
          Open = q.Open,
          Close = q.Close,
          High = q.High,
          Low = q.Low,
          Volume = q.Volume
        }).ToList();

        // Return the data with a 200 OK status
        return Ok(result);
      }
      catch (Exception ex)
      {
        // Return a 400 Bad Request with the error message
        return BadRequest(new { Message = ex.Message });
      }
    }
    // GET: api/yahoofinance/quote?symbols=AAPL,MSFT,GOOGL
    [HttpGet("quote/{symbol}")]
    public async Task<IActionResult> GetQuotes( string symbol)
    {
      try
      {
        var quotes = await YahooService.GetQuotesAsync(symbol);
        return Ok(quotes);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Message = ex.Message });
      }
    }
  }
}
