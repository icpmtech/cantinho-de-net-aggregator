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
using MarketAnalyticHub.Repositories;
using TaskStatus = System.Threading.Tasks.TaskStatus;
using MarketAnalyticHub.Controllers.Configurations.Reddit;
using System.Diagnostics;

namespace MarketAnalyticHub.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  public class PortfolioController : Controller
  {
    private readonly PortfolioService _portfolioService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IYahooFinanceService _yahooFinanceService;
    private readonly IDataRepository _dataRepository; // Para acesso direto, se necessário
    private readonly ILogger<PortfolioController> _logger;
    public PortfolioController(PortfolioService portfolioService, IDataRepository dataRepository, UserManager<ApplicationUser> userManager, IYahooFinanceService yahooFinanceService, ILogger<PortfolioController> logger)
    {
      _portfolioService = portfolioService;
      _userManager = userManager;
      _yahooFinanceService = yahooFinanceService;
      _dataRepository = dataRepository;
      _logger = logger;
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
    // Endpoint para calcular e obter percentuais de um portfólio específico
    [HttpGet("{portfolioId}/percentages")]
    public async Task<IActionResult> GetPortfolioPercentages(int portfolioId)
    {
      var portfolio = await _dataRepository.GetPortfolioByIdAsync(portfolioId);
      if (portfolio == null)
      {
        return NotFound();
      }

      await _portfolioService.CalculatePortfolioMetricsAsync(portfolio);

      var percentages = new
      {
        portfolio.WeeklyPercentage,
        portfolio.MonthlyPercentage,
        portfolio.YearlyPercentage,
        portfolio.LossPercentage,
        portfolio.IsLossAlertTriggered,
        Items = portfolio.Items.Select(item => new
        {
          item.Id,
          item.Symbol,
          item.Quantity,
          item.PurchasePrice,
          item.CurrentPrice,
          item.TotalInvestment,
          item.CurrentMarketValue,
          item.TotalDividendIncome,
          item.PercentChange,
          item.Change,
          item.SentimentImpact,
          item.AdjustedPrice,
         
          item.SectorActivity
        })
      };

      return Ok(percentages);
    }

    // Opcional: Endpoint para obter métricas de um PortfolioItem específico
    [HttpGet("items/{itemId}/metrics")]
    public async Task<IActionResult> GetPortfolioItemMetrics(int itemId)
    {
      var portfolioItem = await _dataRepository.GetPortfolioItemByIdAsync(itemId);
      if (portfolioItem == null)
      {
        return NotFound();
      }

      await _portfolioService.CalculatePortfolioItemMetricsAsync(portfolioItem);

      var metrics = new
      {
        portfolioItem.Id,
        portfolioItem.Symbol,
        portfolioItem.Quantity,
        portfolioItem.PurchasePrice,
        portfolioItem.CurrentPrice,
        portfolioItem.TotalInvestment,
        portfolioItem.CurrentMarketValue,
        portfolioItem.TotalDividendIncome,
        portfolioItem.PercentChange,
        portfolioItem.Change,
        portfolioItem.SentimentImpact,
        portfolioItem.AdjustedPrice,
        portfolioItem.SectorActivity
      };

      return Ok(metrics);
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
      var sw = Stopwatch.StartNew();
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      _logger.LogInformation("GetPortfolios started for UserId={UserId}", userId);

      if (userId == null)
      {
        _logger.LogWarning("Unauthorized access attempt to GetPortfolios");
        return Unauthorized();
      }

      try
      {
        // 1) fetch all portfolios
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

        // 2) process each portfolio concurrently
        var portfolioTasks = portfolios.Select(async portfolio =>
        {
          var innerSw = Stopwatch.StartNew();
          _logger.LogInformation("  Processing PortfolioId={PortfolioId}", portfolio.Id);

          // — Total percentage
          var totalPctTask = _portfolioService.CalculateTotalPortfolioPercentagesAsync(portfolio);

          // — Deduplicate symbols & kick off GetIndustryBySymbol for each
          var uniqueSymbols = portfolio.Items
              .Select(i => i.Symbol)
              .Distinct();
          var industryTasks = uniqueSymbols.ToDictionary(
              sym => sym,
              sym => _portfolioService
                  .GetIndustryBySymbol(sym)
                  .ContinueWith(t => t.Status == TaskStatus.RanToCompletion
                      ? t.Result
                      : "None"));

          // — Weekly/Monthly/Yearly percentage tasks
          var weeklyPctTask = _portfolioService.CalculateWeeklyPortfolioPercentageAsync(portfolio);
          var monthlyPctTask = _portfolioService.CalculateMonthlyPortfolioPercentageAsync(portfolio);
          var yearlyPctTask = _portfolioService.CalculateYearlyPortfolioPercentageAsync(portfolio);

          // wait for all of the above
          await Task.WhenAll(
              totalPctTask,
              Task.WhenAll(industryTasks.Values),
              weeklyPctTask,
              monthlyPctTask,
              yearlyPctTask
          );

          // apply total percentage
          var totalPctResp = await totalPctTask;
          if (totalPctResp != null)
            portfolio.PortfolioPercentage += totalPctResp.TotalDifferencePercentage;

          // map industries back onto items
          foreach (var item in portfolio.Items)
            item.SectorActivity = industryTasks[item.Symbol].Result;

          // assign period percentages
          portfolio.WeeklyPercentage = await weeklyPctTask;
          portfolio.MonthlyPercentage = await monthlyPctTask;
          portfolio.YearlyPercentage = await yearlyPctTask;

          // finally group items by symbol
          portfolio.GroupedItems = portfolio.Items
              .GroupBy(i => i.Symbol)
              .Select(g => new GroupedPortfolioItem
              {
                Symbol = g.Key,
                Items = g.ToList()
              })
              .ToList();

          innerSw.Stop();
          _logger.LogInformation(
              "  Finished PortfolioId={PortfolioId} in {ElapsedMs}ms",
              portfolio.Id,
              innerSw.ElapsedMilliseconds);
        });

        // 3) wait for all portfolios to finish
        await Task.WhenAll(portfolioTasks);

        sw.Stop();
        _logger.LogInformation(
            "GetPortfolios completed for UserId={UserId} in {ElapsedMs}ms",
            userId,
            sw.ElapsedMilliseconds);

        return Ok(portfolios);
      }
      catch (Exception ex)
      {
        sw.Stop();
        _logger.LogError(
            ex,
            "GetPortfolios failed for UserId={UserId} after {ElapsedMs}ms",
            userId,
            sw.ElapsedMilliseconds);
        throw; // or return StatusCode(500);
      }
    }
    [HttpGet("manage-assets")]
    public async Task<IActionResult> GetAssets()
    {
      var sw = Stopwatch.StartNew();
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      _logger.LogInformation("GetPortfolios started for UserId={UserId}", userId);

      if (userId == null)
      {
        _logger.LogWarning("Unauthorized access attempt to GetPortfolios");
        return Unauthorized();
      }

      try
      {
        // 1) fetch all portfolios
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

        sw.Stop();
        _logger.LogInformation(
            "GetPortfolios completed for UserId={UserId} in {ElapsedMs}ms",
            userId,
            sw.ElapsedMilliseconds);

        return Ok(portfolios);
      }
      catch (Exception ex)
      {
        sw.Stop();
        _logger.LogError(
            ex,
            "GetPortfolios failed for UserId={UserId} after {ElapsedMs}ms",
            userId,
            sw.ElapsedMilliseconds);
        throw; // or return StatusCode(500);
      }
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
