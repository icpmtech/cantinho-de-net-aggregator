using DocumentFormat.OpenXml.Presentation;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using Azure;
namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  public class YahooFinanceController : ControllerBase
  {

    private readonly HttpClient _httpClient;

    public YahooFinanceController(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

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

    [HttpGet("customer-analysis/{symbol}")]
    public async Task<IActionResult> GetCustomerData(string symbol)
    {
      var url = $"https://csimarket.com/stocks/markets_glance.php?code={symbol}";

      try
      {
        // Fetch the HTML content from the URL
        var responseMessage = await _httpClient.GetAsync(url);
        responseMessage.EnsureSuccessStatusCode(); // Ensure the request was successful

        // Read the content as a byte array and decode it with UTF-8
        var responseBytes = await responseMessage.Content.ReadAsByteArrayAsync();
        var responseContent = Encoding.UTF8.GetString(responseBytes);

        // Load HTML into HtmlAgilityPack for parsing
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);

        // Extract data
        var customerData = new ModelCustomerData
        {
          Symbol = symbol,
          CompanyName = ExtractCompanyName(htmlDoc),
          Sector = ExtractSector(htmlDoc),
          Industry = ExtractIndustry(htmlDoc),
          RevenueByDivision = ExtractRevenueByDivision(htmlDoc),
          RevenueGrowthByIndustry = ExtractRevenueGrowthByIndustry(htmlDoc),
          CustomerPerformanceMetrics = ExtractCustomerPerformanceMetrics(htmlDoc)
        };

        return Ok(customerData);
      }
      catch (HttpRequestException)
      {
        return StatusCode(500, "Error fetching data from csimarket.com");
      }
    }

    private string ExtractCompanyName(HtmlDocument doc)
    {
      var node = doc.DocumentNode.SelectSingleNode("//div[@class='naslovcompet3']/h1");
      return node?.InnerText.Trim() ?? "N/A";
    }

    private string ExtractSector(HtmlDocument doc)
    {
      var node = doc.DocumentNode.SelectSingleNode("//span[@class='sector']");
      return node?.InnerText.Trim() ?? "N/A";
    }

    private string ExtractIndustry(HtmlDocument doc)
    {
      var node = doc.DocumentNode.SelectSingleNode("//span[@class='industry']");
      return node?.InnerText.Trim() ?? "N/A";
    }

    private Dictionary<string, string> ExtractRevenueByDivision(HtmlDocument doc)
    {
      var divisions = new Dictionary<string, string>();
      var divisionNodes = doc.DocumentNode.SelectNodes("//table//tr[@onmouseover='this.className=\"bgplv\"']");
      if (divisionNodes != null)
      {
        foreach (var node in divisionNodes)
        {
          var divisionName = node.SelectSingleNode(".//td[@class='segtxt']")?.InnerText.Trim();
          var revenuePercentage = node.SelectSingleNode(".//td[@class='deschardistance']")?.InnerText.Trim();

          if (!string.IsNullOrEmpty(divisionName) && !string.IsNullOrEmpty(revenuePercentage))
          {
            divisions[divisionName] = revenuePercentage;
          }
        }
      }
      return divisions;
    }
    private List<IndustryGrowth> ExtractRevenueGrowthByIndustry(HtmlDocument doc)
    {
      var growthData = new List<IndustryGrowth>();
      var industryNodes = doc.DocumentNode.SelectNodes("//table//tr[@valign='middle']");
      if (industryNodes != null)
      {
        foreach (var node in industryNodes)
        {
          var industryName = node.SelectSingleNode(".//td[@class='segtxt']/a")?.InnerText.Trim();
          var growthPercentage = node.SelectSingleNode(".//td[@class='lijevchar b lijevcharfont']")?.InnerText.Trim();

          if (!string.IsNullOrEmpty(industryName) && !string.IsNullOrEmpty(growthPercentage))
          {
            growthData.Add(new IndustryGrowth
            {
              Industry = industryName,
              GrowthPercentage = growthPercentage
            });
          }
        }
      }
      return growthData;
    }

    private CustomerPerformanceMetrics ExtractCustomerPerformanceMetrics(HtmlDocument doc)
    {
      var netIncomeGrowth = doc.DocumentNode.SelectSingleNode("//span[@class='zv']")?.InnerText.Trim() ?? "N/A";
      var netMarginGrowth = doc.DocumentNode.SelectSingleNode("//span[@class='zv']")?.InnerText.Trim() ?? "N/A";
      return new CustomerPerformanceMetrics
      {
        NetIncomeGrowth = netIncomeGrowth,
        NetMarginGrowth = netMarginGrowth
      };
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

    // GET: api/yahoofinance/search-news/{query}
    [HttpGet("search-news/{query}")]
    public async Task<IActionResult> SearchNews(string query)
    {
      try
      {
        var symbols = await YahooService.SearchNewsAsync(query);
        return Ok(symbols);
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
    // GET: api/yahoofinance/summary/AAPL
    [HttpGet("summary/{symbol}")]
    public async Task<IActionResult> GetSummaryRawAssetProfile(string symbol)
    {
      try
      {
        var summary = await YahooService.GetSummaryRawAsync(symbol);
        return Ok(summary);
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


    [HttpGet("stock/{symbol}")]
    public async Task<IActionResult> GetStockData(string symbol, [FromQuery] string interval = "5m")
    {
      try
      {
        DateTime endDate = DateTime.Now;
        DateTime startDate = endDate.AddDays(-1);

        // Call the service to get historical data
        var quotes = await YahooService.GetHistoricalDataAsync(symbol, startDate, endDate, interval);

        // Transform the data to the ViewModel with safe conversion and null handling
        var result = quotes.Select(q => new HistoricalQuoteViewModel
        {
          Timestamp = q.Timestamp ?? DateTime.UtcNow.Ticks,
          Open = TryConvertToDecimal(q.Open),
          Close = TryConvertToDecimal(q.Close),
          High = TryConvertToDecimal(q.High),
          Low = TryConvertToDecimal(q.Low),
          Volume = TryConvertToDecimal(q.Volume) // Assuming Volume is a decimal
        }).ToList();

        return Ok(result);
      }
      catch (Exception ex)
      {
        // Log the exception details here if necessary
        // _logger.LogError(ex, "An error occurred while fetching stock data.");

        return BadRequest(new { Message = "An error occurred while processing your request. Please try again later." });
      }
    }

    // Helper method to safely convert values to decimal
    private decimal TryConvertToDecimal(object value)
    {
      if (value == null) return 0;

      try
      {
        // Convert to string and then parse to decimal
        return decimal.Parse(value.ToString(), System.Globalization.NumberStyles.Any);
      }
      catch (FormatException)
      {
        // Log conversion error if needed
        return 0; // Default value on conversion error
      }
      catch (InvalidCastException)
      {
        // Log casting error if needed
        return 0; // Default value on casting error
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
    [HttpGet("chart-symbol/{symbol}")]
    public async Task<IActionResult> GetSymbolsChartSearch(string symbol, DateTime? startDate = null, DateTime? endDate = null, string interval = "5m")
    {
      try
      {
        // Use the provided startDate and endDate, or default to one year range ending today
        var finalEndDate = endDate ?? DateTime.Now;
        var finalStartDate = startDate ?? finalEndDate.AddYears(-1);

        // Call the service to get historical data
        var quotes = await YahooService.GetHistoricalDataAsync(symbol, finalStartDate, finalEndDate, interval);

        var result = quotes.Select(q => new HistoricalQuoteViewModel
        {
          Timestamp = q.Timestamp ?? DateTime.UtcNow.Ticks,
          Open = TryConvertToDecimal(q.Open),
          Close = TryConvertToDecimal(q.Close),
          High = TryConvertToDecimal(q.High),
          Low = TryConvertToDecimal(q.Low),
          Volume = TryConvertToLong(q.Volume),
        }).ToList();

        return Ok(result);

       
      }
      catch (Exception ex)
      {
        // Return a 400 Bad Request with the error message
        return BadRequest(new { Message = ex.Message });
      }
    }

    private long TryConvertToLong(dynamic value)
    {
      if (value == null) return 0;

      try
      {
        // Convert to string and then parse to decimal
        return long.Parse(value.ToString(), System.Globalization.NumberStyles.Any);
      }
      catch (FormatException)
      {
        // Log conversion error if needed
        return 0; // Default value on conversion error
      }
      catch (InvalidCastException)
      {
        // Log casting error if needed
        return 0; // Default value on casting error
      }
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

     

        // Return the data with a 200 OK status
        return Ok(quotes);
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
