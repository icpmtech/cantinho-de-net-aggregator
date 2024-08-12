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
  }
}