namespace MarketAnalyticHub.Services
{
  using MarketAnalyticHub.Controllers.api;
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Services.ApiDataApp.Services;
  using MarketAnalyticHub.YaooServive.Models;
  using Microsoft.Graph;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using YahooFinanceApi;

  using Microsoft.AspNetCore.Mvc;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using DividendApi.Services;

  [ApiController]
    [Route("api/search-dividends-yhaoo")]
    public class DividendController : ControllerBase
    {
      private readonly IDividendScraper _dividendScraper;

      public DividendController(IDividendScraper dividendScraper)
      {
        _dividendScraper = dividendScraper;
      }

      /// <summary>
      /// Retrieves the dividend history for a given stock symbol within a date range.
      /// </summary>
      /// <param name="symbol">Stock symbol (e.g., AAPL)</param>
      /// <param name="start">Start date in yyyy-MM-dd format</param>
      /// <param name="end">End date in yyyy-MM-dd format</param>
      /// <returns>List of dividend records</returns>
      [HttpGet]
      public async Task<IActionResult> GetDividends([FromQuery] string symbol, [FromQuery] string start, [FromQuery] string end)
      {
        if (string.IsNullOrEmpty(symbol))
        {
          return BadRequest("Stock symbol is required.");
        }

        if (!DateTime.TryParse(start, out DateTime startDate))
        {
          return BadRequest("Invalid start date format. Use yyyy-MM-dd.");
        }

        if (!DateTime.TryParse(end, out DateTime endDate))
        {
          return BadRequest("Invalid end date format. Use yyyy-MM-dd.");
        }

        if (startDate > endDate)
        {
          return BadRequest("Start date cannot be after end date.");
        }

        try
        {
          List<DividendRecord> dividends = await _dividendScraper.GetDividendHistoryAsync(symbol.ToUpper(), startDate, endDate);
          return Ok(dividends);
        }
        catch (Exception ex)
        {
          // Log the exception (not implemented here)
          return StatusCode(500, $"Internal server error: {ex.Message}");
        }
      }

    }
  }



