using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarketAnalyticHub.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketAnalyticHub.Models.Portfolio;
using AspnetCoreMvcFull.Models;
using static MarketAnalyticHub.Models.Portfolio.Portfolio;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class DashboardsController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly PortfolioService _portfolioService;
    private readonly IYahooFinanceService _yahooFinanceService;
    public DashboardsController(ApplicationDbContext context, PortfolioService portfolioService, IYahooFinanceService yahooFinanceService)
    {
      _context = context;
      _portfolioService = portfolioService;
      _yahooFinanceService = yahooFinanceService;
    }
    [HttpGet("chartdata-date-range/{id}")]
    public async Task<IActionResult> GetChartDataDateRange(int id, [FromQuery] string timeRange)
    {
      var portfolioItem = await _context.PortfolioItems
               .FirstOrDefaultAsync(p => p.Id == id);

      if (portfolioItem == null)
      {
        return NotFound();
      }

      DateTime startDate = timeRange switch
      {
        "1d" => DateTime.Now.AddDays(-1),
        "1w" => DateTime.Now.AddDays(-7),
        "1m" => DateTime.Now.AddMonths(-1),
        "3m" => DateTime.Now.AddMonths(-3),
        "6m" => DateTime.Now.AddMonths(-6),
        "1y" => DateTime.Now.AddYears(-1),
        "5y" => DateTime.Now.AddYears(-5),
        "all" => DateTime.Now.AddYears(-50),
        _ => DateTime.Now.AddMonths(-1) // Default to "all" if no valid range is specified
      };

      var data = timeRange switch
      {
        "1d" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now,YahooFinanceApi.Period.Daily),
        "1w" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now, YahooFinanceApi.Period.Daily),
        "1m" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now,YahooFinanceApi.Period.Weekly),
        "3m" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now, YahooFinanceApi.Period.Weekly),
        "6m" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now, YahooFinanceApi.Period.Monthly),
        "1y" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now, YahooFinanceApi.Period.Monthly),
        "5y" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now, YahooFinanceApi.Period.Monthly),
        "all" => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now, YahooFinanceApi.Period.Monthly),
        _ => await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, startDate, DateTime.Now, YahooFinanceApi.Period.Daily)
      };

      var dataResult = new
      {
        dates = data.Select(h => h.Date.ToString(timeRange == "1d" ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd")).ToArray(),
        opens = data.Select(h => h.Open).ToArray(),
        highs = data.Select(h => h.High).ToArray(),
        lows = data.Select(h => h.Low).ToArray(),
        closes = data.Select(h => h.Close).ToArray(),
        volumes = data.Select(h => h.Volume).ToArray()
      };

      return Ok(dataResult);
    }
    [HttpGet("chartdata/{id}")]
      public async Task<IActionResult> GetChartData(int id)
      {
        var portfolioItem = await _context.PortfolioItems
            .FirstOrDefaultAsync(p => p.Id == id);

        if (portfolioItem == null)
        {
          return NotFound();
        }
      var data = await _yahooFinanceService.GetHistoricalDataAsync(portfolioItem.Symbol, DateTime.Now.AddDays(-30), DateTime.Now,YahooFinanceApi.Period.Daily);
      var dataResult = new
      {
        dates = data.Select(h => h.Date.ToString("yyyy-MM-dd")).ToArray(),
        opens = data.Select(h => h.Open).ToArray(),
        highs = data.Select(h => h.High).ToArray(),
        lows = data.Select(h => h.Low).ToArray(),
        closes = data.Select(h => h.Close).ToArray(),
        volumes = data.Select(h => h.Volume).ToArray()
      };

      return Ok(dataResult);
      }

      [HttpGet("data")]
    public async Task<ActionResult<DashboardData>> GetDashboardData()
    {
      // Replace "userId" with the actual user ID from your authentication context
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

      var data = await GetDashboardDataAsync(userId);
      return Ok(data);
    }
    [HttpGet("income")]
    public async Task<IActionResult> GetIncomeData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
      var incomeData = new
      {
        series = portfolios.Select(s => PortfolioBaseService.CalculateMonthlySummaries(s.Items).OrderBy(ms => ms.Key).Select(ms => ms.Value.CurrentMarketValue)).ToArray(),
        categories = portfolios.Select(s => PortfolioBaseService.CalculateMonthlySummaries(s.Items).OrderBy(ms => ms.Key).Select(ms => ms.Key.ToString("MMM yyyy")).ToArray())
      };

      return Ok(incomeData);
    }

    [HttpGet("dividends")]
    public async Task<IActionResult> GetDividendsData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

      var dividendsData = new
      {
        series = portfolios.Select(s=> PortfolioBaseService.CalculateMonthlySummaries(s.Items).OrderBy(ms => ms.Key).Select(ms => ms.Value.TotalDividendIncome).ToArray()),
        categories = portfolios.Select(s => PortfolioBaseService.CalculateMonthlySummaries(s.Items).OrderBy(ms => ms.Key).Select(ms => ms.Key.ToString("MMM yyyy")).ToArray())
      };

      return Ok(dividendsData);
    }

    private async Task<decimal> GetCurrentPriceAsync(string symbol)
    {
      // Implement this method to fetch the current price of the symbol
      return await Task.FromResult(100m); // Example price
    }
    // GET: api/Dashboards/revenue
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueData()
    {
      var revenueData = new
      {
        Series2021 = new[] { 18, 7, 15, 29, 18, 12, 9 },
        Series2020 = new[] { -13, -18, -9, -14, -5, -17, -15 }
      };

      return Ok(revenueData);
    }

    // GET: api/Dashboards/expenses
    [HttpGet("expenses")]
    public async Task<IActionResult> GetEventsDataWeek()
    {
      try
      {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

        // Get the start date of the current week (assuming week starts on Monday)
        var startOfWeek = PortfolioHelpers.GetStartOfWeek(DateTime.Now);

        var lastWeekEvents = portfolios
            .SelectMany(p => p.Items.SelectMany(i => i.StockEvents))
            .Where(e => DateTime.Parse(e.Date) >= startOfWeek && DateTime.Parse(e.Date) < startOfWeek.AddDays(7))
            .ToList();

        var eventsData = new
        {
          Series = lastWeekEvents.Count()
        };

        return Ok(eventsData);
      }
      catch (Exception ex)
      {
        // Log the exception (ex)
        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing your request." });
      }
    }
    // GET: api/Dashboards/profit-week
    [HttpGet("profit-week")]
    public async Task<IActionResult> GetProfitDataWek()
    {
      // Replace the hardcoded data with actual data retrieval logic
      try
      {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);


        var allWeeklySummaries = PortfolioHelpers.AggregateWeeklySummaries(portfolios);

        // Get the summary for the last week
        var lastWeekSummary = allWeeklySummaries.OrderByDescending(kv => kv.Key).FirstOrDefault();

        if (lastWeekSummary.Value.Summary == null)
        {
          return NotFound(new { message = "No data available for the last week." });
        }

        var profitData = new
        {
          Series = lastWeekSummary.Value.Summary.TotalInvestment- lastWeekSummary.Value.Summary.CurrentMarketValue // Example data
        };

        return Ok(profitData);
      }
      catch (Exception ex)
      {
        // Log the exception (ex)
        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing your request." });
      }
    }

    // GET: api/Dashboards/dividends-week
    [HttpGet("dividends-week")]
    public async Task<IActionResult> GetDividendsDataWeek()
    {
      try
      {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
        var allWeeklySummaries = PortfolioHelpers.AggregateWeeklySummaries(portfolios);

        // Get the summary for the last week
        var lastWeekSummary = allWeeklySummaries.OrderByDescending(kv => kv.Key).FirstOrDefault();

        if (lastWeekSummary.Value.Summary == null)
        {
          return NotFound(new { message = "No data available for the last week." });
        }

        var profitData = new
        {
          Series = lastWeekSummary.Value.Summary.TotalDividendIncome // Example data
        };

        return Ok(profitData);
      }
      catch (Exception ex)
      {
        // Log the exception (ex)
        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing your request." });
      }
    }

   


    // GET: api/Dashboards/profit
    [HttpGet("profit")]
    public async Task<IActionResult> GetProfitData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

      var allMonthlySummaries = new Dictionary<DateTime, MonthlyPortfolioSummary>();

      foreach (var portfolio in portfolios)
      {
        var monthlySummaries = PortfolioBaseService.CalculateMonthlySummaries(portfolio.Items);
        foreach (var summary in monthlySummaries)
        {
          if (allMonthlySummaries.ContainsKey(summary.Key))
          {
            allMonthlySummaries[summary.Key].TotalInvestment += summary.Value.TotalInvestment;
            allMonthlySummaries[summary.Key].CurrentMarketValue += summary.Value.CurrentMarketValue;
            allMonthlySummaries[summary.Key].TotalDividendIncome += summary.Value.TotalDividendIncome;
          }
          else
          {
            allMonthlySummaries[summary.Key] = summary.Value;
          }
        }
      }

      // Transform the allMonthlySummaries to an array of current market values
      var profitData = new
      {
        Data = allMonthlySummaries.OrderBy(kv => kv.Key)
                                    .Select(kv => kv.Value.CurrentMarketValue)
                                    .ToArray()
      };

      return Ok(profitData);
    }
    // GET: api/Dashboards/symbols-statistics
    [HttpGet("symbols-statistics")]
    public async Task<IActionResult> GetSymbolsV1Statistics()
    {
      // Replace the hardcoded data with actual data retrieval logic
      var orderStatistics = new
      {
        Labels = new[] { "Electronic", "Sports", "Decor", "Fashion" },
        Series = new[] { 85, 15, 50, 50 }
      };

      return Ok(orderStatistics);
    }

    // GET: api/Dashboards/growth
    [HttpGet("growth")]
    public async Task<IActionResult> GetGrowthData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch the portfolios first
      decimal growthPercentage = await _portfolioService.GrowthPercentage(userId);

      var growthData = new
      {
        Growth = Math.Round(growthPercentage, 3)
      };

      return Ok(growthData);
    }

    
    [HttpGet("GetPortfoliosByUserAsync")]
    public async Task<IEnumerable<Portfolio>> GetPortfoliosByUserAsync(string userId)
    {
      var portfolios = await _context.Portfolios
                                     .Include(p => p.Items)
                                     .ThenInclude(pi => pi.Dividends)
                                     .Where(p => p.UserId == userId)
                                     .ToListAsync();

      // Update current prices and calculate fields
      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          item.CurrentPrice = await GetCurrentPriceAsync(item.Symbol); // Implement this method to fetch the current price
        }
      }

      return portfolios;
    }
    [HttpGet("portfolio-statistics")]
    public async Task<ActionResult<IEnumerable<PortfolioStatistic>>> GetPortfolioStatistics()
    {
      // Replace with actual user ID from your authentication context
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var overallStats = await _portfolioService.GetTotalPortfolioOverall(userId);
      var portfolios = await _context.Portfolios
                                      .Include(p => p.Items)
                                      .ThenInclude(pi => pi.Dividends)
                                      .Where(p => p.UserId == userId)
                                      .ToListAsync();
     
      var portfolioStatistics = portfolios.Select(p => new PortfolioStatistic
      {
        Name = p.Name,
        TotalInvestment = (decimal)overallStats.TotalMarketValue ,
        CurrentMarketValue = (decimal)overallStats.TotalCustMarketValue,
        TotalDifferenceValue = (decimal)overallStats.TotalDifferenceValue,
        TotalDividends = (decimal)overallStats.TotalDividends,
        TotalProfit = (decimal)overallStats.TotalPortfolioProfit,
        TotalDifferencePercentage = (decimal)overallStats.TotalDifferencePercentage,
        TotalProfitDifferencePercentage = (decimal)overallStats.TotalDifferenceWithDividendsPercentage,
      });
      
      return Ok(portfolioStatistics);
    }

    [HttpGet("portfolio-statistics-overall")]
    public async Task<ActionResult<PortfolioStatistic>> GetPortfolioStatisticsOverall()
    {
      // Replace with actual user ID from your authentication context
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var overallStats = await _portfolioService.GetTotalPortfolioOverall(userId);
      // Get portfolios for the user along with their items and dividends
      var portfolios = await _context.Portfolios
                                      .Include(p => p.Items)
                                      .ThenInclude(pi => pi.Dividends)
                                      .Where(p => p.UserId == userId)
                                      .ToListAsync();



      var overallStatistic = new PortfolioStatistic
      {
        Name = "Overall Portfolio",
        TotalInvestment = overallStats.TotalCustMarketValue,
        CurrentMarketValue = overallStats.TotalMarketValue,
        TotalDifferenceValue = overallStats.TotalDifferenceValue,
        TotalDividends = overallStats.TotalDividends,
        TotalProfit = overallStats.TotalPortfolioProfit,
        TotalDifferencePercentage = overallStats.TotalDifferencePercentage,
        TotalProfitDifferencePercentage = overallStats.TotalDifferenceWithDividendsPercentage
      };

      return Ok(overallStatistic);
    }


    private async Task<DashboardData> GetDashboardDataAsync(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);
      var overallStats = await _portfolioService.GetTotalPortfolioOverall(userId);
      decimal totalInvestment = (decimal)overallStats.TotalCustMarketValue;
      decimal currentMarketValue = (decimal)overallStats.TotalMarketValue;
      decimal dividends = portfolios.Sum(p => p.Items.Sum(i => i.Dividends.Sum(d => d.Amount)));
      decimal profit = currentMarketValue - totalInvestment;
      int? payments = await _portfolioService.TotalStockEvents(userId); ; // This should be replaced with actual payments data
      int? operations = await _portfolioService.TotalTransactions(userId); // This should be replaced with actual operations data
      decimal yearlyReport = 84686; // This should be replaced with actual yearly report data
      decimal growth = totalInvestment > 0 ? (profit / totalInvestment) * 100 : 0;
      
      decimal portfolioGrowth = currentMarketValue > 0 ? ((decimal)overallStats.TotalMarketValue / currentMarketValue) * 100 : 0;
      return new DashboardData
      {
        Profit = profit,
        ProfitPercentage= overallStats.TotalDifferencePercentage,
        Dividends = dividends,
        DividendsPercentage= overallStats.TotalDifferencePercentage,
        Payments = payments,
        Operations = operations,
        TotalRevenue = (decimal)overallStats.TotalMarketValue, // Example data
        Growth = growth,
        PortfolioGrowth = portfolioGrowth,
        YearlyReport = yearlyReport
      };
    }
  }


}
