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

      var data = await _yahooFinanceService.GetHistoricalDataAsync(symbol, startDate, endDate);

      return Ok(data);
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

    [HttpPost("Import")]
    public async Task<IActionResult> Import([FromForm] IFormFile file)
    {
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
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
      foreach (var portfolio in portfolios)
      {
        var portfolioPercentageResponse = _portfolioService.CalculatePortfolioPercentages(portfolio);
        if(portfolioPercentageResponse is not null)
        portfolio.PortfolioPercentage += portfolioPercentageResponse.TotalDifferencePercentage;
        // Group portfolio items by symbol
        var groupedItems = portfolio.Items
            .GroupBy(item => item.Symbol)
            .Select(group => new
            {
              Symbol = group.Key,

              Items = group.Distinct().ToList()
            });

        // You can then replace portfolio.Items with the grouped items if needed
        // Or perform further operations on the grouped items
        portfolio.GroupedItems = groupedItems.ToList(); // Assuming GroupedItems is a new property in your portfolio model

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePortfolio(int id)
    {
      await _portfolioService.DeletePortfolioAsync(id);
      return NoContent();
    }
    [HttpPost("calculateportfoliopercentages")]
    public async Task<ActionResult<PortfolioPercentageResponse>> CalculatePortfolioPercentages([FromBody] int portfolioId)
    {
      var portfolio = await _portfolioService.GetPortfolioByIdAsync(portfolioId);
      if (portfolio == null)
      {
        return NotFound("Portfolio not found");
      }

      var response = _portfolioService.CalculatePortfolioPercentages(portfolio);
      return Ok(response);
    }
    [HttpGet("total-percentage")]
    public async Task<IActionResult> GetTotalPortfolioPercentage()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

      double totalPercentage = 0;
      double totalDifferenceWithDividendsPercentage = 0;
      double totalProfit = 0;
      foreach (var portfolio in portfolios)
      {
        var portfolioPercentageResponse = _portfolioService.CalculatePortfolioPercentages(portfolio);
        if(portfolioPercentageResponse is not null)
        totalPercentage += portfolioPercentageResponse.TotalDifferencePercentage;
        totalDifferenceWithDividendsPercentage += portfolioPercentageResponse.TotalDifferenceWithDividendsPercentage;
        totalProfit += portfolioPercentageResponse.TotalPortfolioProfit;
      }

      return Ok(new { TotalPercentage = totalPercentage,TotalProfit= totalProfit, TotalWithDividendsPercentage = totalDifferenceWithDividendsPercentage });
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
