using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using MarketAnalyticHub.Controllers.api;
using Microsoft.Graph;
using System.Globalization;

namespace MarketAnalyticHub.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  public class PortfolioController : Controller
  {
    private readonly PortfolioService _portfolioService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IYahooFinanceService _yahooFinanceService;

    public PortfolioController(PortfolioService portfolioService, UserManager<ApplicationUser> userManager, IYahooFinanceService yahooFinanceService)
    {
      _portfolioService = portfolioService;
      _userManager = userManager;
      _yahooFinanceService = yahooFinanceService;
    }

    

    [HttpGet("purchase-dates-for-symbol")]
    public async Task<IActionResult> GetPurchaseDatesForSymbol([FromQuery] string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var purchaseDates = await _portfolioService.GetPurchaseDatesForSymbol(userId, symbol, startDate, endDate);
      if (purchaseDates == null || !purchaseDates.Any())
      {
        return NotFound("No purchase data found for the specified criteria.");
      }
      return Ok(purchaseDates);
    }
    [HttpGet("historical-data")]
    public async Task<IActionResult> GetHistoricalData([FromQuery] string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
      if (string.IsNullOrEmpty(symbol))
      {
        return BadRequest("Symbol is required.");
      }

      var data = await YahooService.GetHistoricalDataAsync(symbol, startDate, endDate, "1wk");

      if (data == null || !data.Any())
      {
        return Ok(new { Message = "No historical data found", Data = 0 });
      }
      // Ensure culture is set to InvariantCulture
      CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
      // Create the DataResult object
      var dataResult = new DataResult
      {
        Dates = data.Select(h => h.Timestamp.ToString("yyyy-MM-dd")).OfType<string>().ToArray(),
        Opens = data.Select(h => ConvertToDecimal(h.Open)).OfType<decimal>().ToArray(),
        Highs = data.Select(h => ConvertToDecimal(h.High)).OfType<decimal>().ToArray(),
        Lows = data.Select(h => ConvertToDecimal(h.Low)).OfType<decimal>().ToArray(),
        Closes = data.Select(h => ConvertToDecimal(h.Close)).OfType<decimal>().ToArray(),
        Volumes = data.Select(h => ConvertToDecimal(h.Volume)).OfType<decimal>().ToArray()
      };
      return Ok(dataResult);
    }
    private decimal ConvertToDecimal(dynamic value)
    {
      if (value == null)
        return 0m; // or throw an exception or handle as appropriate

      try
      {
        // Use invariant culture to handle decimal separators correctly
        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        // Handle conversion errors
        return 0m; // or handle as appropriate
      }
    }

    [HttpGet("stock-price")]
    public async Task<IActionResult> GetRealTimePrice([FromQuery] string symbol)
    {
      if (string.IsNullOrEmpty(symbol))
      {
        return BadRequest("Symbol is required.");
      }

      var data = await _yahooFinanceService.GetRealTimePriceAsync(symbol);

      return Ok(data);
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export([FromQuery] string fileType)
    {
      var userId = _userManager.GetUserId(User);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

      if (fileType == "csv")
      {
        var csvData = _portfolioService.ExportToCsv(portfolios);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
        return File(bytes, "text/csv", "portfolios.csv");
      }
      else
      {
        using (var workbook = _portfolioService.ExportToExcel(portfolios))
        {
          using (var stream = new MemoryStream())
          {
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "portfolios.xlsx");
          }
        }
      }
    }
    [HttpPost("ImportYahoo")]
    public async Task<IActionResult> ImportYahoo([FromForm] ImportYahooRequest importYahooRequest)
    {
      var file = importYahooRequest.File;
      if (file == null || file.Length == 0)
        return BadRequest("File is empty");

      var userId = _userManager.GetUserId(User);
      var extension = Path.GetExtension(file.FileName).ToLower();

      using (var stream = new MemoryStream())
      {
        await file.CopyToAsync(stream);
        stream.Position = 0;

        if (extension == ".csv")
        {
          using (var reader = new StreamReader(stream))
          {
            var csvData = await reader.ReadToEndAsync();
            await _portfolioService.ImportYahooFromCsv(csvData, userId);
          }
        }
        else if (extension == ".xlsx")
        {
          await _portfolioService.ImportYahooFromExcel(stream, userId);
        }
        else
        {
          return BadRequest("Unsupported file format");
        }
      }

      return Ok();
    }


    [HttpPost("Import")]
    public async Task<IActionResult> Import([FromForm] ImportRequest importRequest)
    {
      var file = importRequest.File;
      if (file == null || file.Length == 0)
        return BadRequest("File is empty");

      var userId = _userManager.GetUserId(User);
      var extension = Path.GetExtension(file.FileName).ToLower();

      using (var stream = new MemoryStream())
      {
        await file.CopyToAsync(stream);
        stream.Position = 0;

        if (extension == ".csv")
        {
          using (var reader = new StreamReader(stream))
          {
            var csvData = await reader.ReadToEndAsync();
            await _portfolioService.ImportFromCsv(csvData, userId);
          }
        }
        else if (extension == ".xlsx")
        {
          await _portfolioService.ImportFromExcel(stream, userId);
        }
        else
        {
          return BadRequest("Unsupported file format");
        }
      }

      return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetPortfolios()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

      foreach (var portfolio in portfolios)
      {
        var portfolioPercentageResponse = await _portfolioService.CalculateTotalPortfolioPercentagesAsync(portfolio);
        if (portfolioPercentageResponse != null)
        {
          portfolio.PortfolioPercentage += portfolioPercentageResponse.TotalDifferencePercentage;
        }

        // Group portfolio items by symbol
        var groupedItems = portfolio.Items
            .GroupBy(item => item.Symbol)
            .Select(group => new GroupedPortfolioItem
            {
              Symbol = group.Key,
              Items = group.ToList()
            })
            .ToList();

        // Assign grouped items to the portfolio
        portfolio.GroupedItems = groupedItems;
      }

      return Ok(portfolios);
    }


    [HttpPost("CalculateOriginalMarketValue")]
    public ActionResult<MarketValueResponse> CalculateOriginalMarketValue([FromBody] MarketValueRequest request)
    {
      if (request == null)
      {
        return BadRequest("Invalid request");
      }

      var originalMarketValue = _portfolioService.CalculateOriginalMarketValue(request.CurrentMarketValue, request.PercentageIncrease);
      return Ok(new MarketValueResponse { OriginalMarketValue = originalMarketValue });
    }

    [HttpPost("CalculatePercentageChange")]
    public ActionResult<PercentageChangeResponse> CalculatePercentageChange([FromBody] PercentageChangeRequest request)
    {
      if (request == null)
      {
        return BadRequest("Invalid request");
      }

      var percentageChange = _portfolioService.CalculatePercentageChange(request.OriginalMarketValue, request.CurrentMarketValue);
      return Ok(new PercentageChangeResponse { PercentageChange = percentageChange });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPortfolio(int id)
    {
      var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
      if (portfolio == null)
      {
        return NotFound();
      }
      return Ok(portfolio);
    }

    [HttpPost]
    public async Task<IActionResult> AddPortfolio([FromBody] Portfolio portfolio)
    {
      portfolio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      await _portfolioService.AddPortfolioAsync(portfolio);
      return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] Portfolio portfolio)
    {
      portfolio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (id != portfolio.Id)
      {
        return BadRequest();
      }
      await _portfolioService.UpdatePortfolioAsync(portfolio);
      return NoContent();
    }

    [HttpGet("stock-candlestick-data")]
    public async Task<IActionResult> GetCandlestickData([FromQuery] string symbol, [FromQuery] string resolution = "D", [FromQuery] int count = 5)
    {
      if (string.IsNullOrEmpty(symbol))
      {
        return BadRequest("Symbol is required.");
      }

      var data = await _portfolioService.GetCandlestickDataAsync(symbol, resolution, count);

      return Ok(data);
    }


    [HttpDelete()]
    public async Task<IActionResult> DeleteAllPortfolios()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      await _portfolioService.DeletePortfolioAllAsync(userId);
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePortfolio(int id)
    {
      await _portfolioService.DeletePortfolioAsync(id);
      return NoContent();
    }


    [HttpGet("total-percentage")]
    public async Task<IActionResult> GetTotalPortfolioPercentage()
    {
      // Replace with actual user ID from your authentication context
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

      if (string.IsNullOrEmpty(userId))
      {
        return Unauthorized("User not found");
      }

      var overallStats = await _portfolioService.GetTotalPortfolioOverall(userId);

      if (overallStats == null)
      {
        return NotFound("No portfolio statistics found for the user");
      }

      var portfolioStatistic = new PortfolioStatistic
      {
        TotalInvestment = overallStats.TotalCustMarketValue,
        CurrentMarketValue = overallStats.TotalMarketValue,
        TotalDifferenceValue = overallStats.TotalDifferenceValue,
        TotalDividends = overallStats.TotalDividends,
        TotalProfit = overallStats.TotalPortfolioProfit,
        TotalDifferencePercentage = overallStats.TotalDifferencePercentage,
        TotalProfitDifferencePercentage = overallStats.TotalDifferenceWithDividendsPercentage
      };

      return Ok(portfolioStatistic);
    }


    [HttpGet("portfolio-overall-stats")]
    public async Task<IActionResult> GetTotalPortfolioOverallStats()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var overallStats = await _portfolioService.GetTotalPortfolioOverall(userId);

      return Ok(overallStats);
    }

      }
}
