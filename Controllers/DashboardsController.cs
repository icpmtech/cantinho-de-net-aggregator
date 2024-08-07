using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models.Portfolio;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketAnalyticHub.Services;
using System;
using System.Collections.Generic;
using MarketAnalyticHub.Models.Dashboard;
using MarketAnalyticHub.Models;
using System.Net.Http;
using MarketAnalyticHub.Controllers.api;

namespace MarketAnalyticHub.Controllers
{
  [Authorize]
  public class DashboardsController : Controller
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

    public IActionResult GetTransactions(string filter)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch the portfolios first
      var portfolios = _context.Portfolios
          .Where(p => p.UserId == userId)
          .Include(p => p.Items) // Include the related items
          .ToList();

      DateTime fromDate = DateTime.MinValue;

      if (filter == "Last 28 Days")
      {
        fromDate = DateTime.Now.AddDays(-28);
      }
      else if (filter == "Last Month")
      {
        fromDate = DateTime.Now.AddMonths(-1);
      }
      else if (filter == "Last Year")
      {
        fromDate = DateTime.Now.AddYears(-1);
      }
      else if (filter == "All")
      {
        fromDate = DateTime.Now.AddYears(-50);
      }

      var transactions = portfolios
          .SelectMany(p => p.Items)
          .Where(item => item.PurchaseDate >= fromDate)
          .Select(item => new TransactionDto
          {
            Type = item.OperationType,
            Description = item.Symbol,
            Icon = PortfolioHelpers.GetIconForTransaction(item.Symbol),
            Amount = item.Quantity * item.PurchasePrice,
            Currency = "€",
            Date = item.PurchaseDate,
            Source = item.Symbol
          })
          .ToList();

      return PartialView("_TransactionsPartial", transactions);
    }
    public async Task<IActionResult> GetPortfolioStatistics_v2()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch overall portfolio statistics
      var overallStats = await _portfolioService.GetTotalPortfolioOverall(userId);
      if (overallStats == null)
      {
        return NotFound("No portfolio statistics found.");
      }

      // Fetch user portfolios with related data
      var portfolios = await _context.Portfolios
                                     .Include(p => p.Items)
                                     .ThenInclude(pi => pi.Dividends)
                                     .Where(p => p.UserId == userId)
                                     .ToListAsync();

      if (!portfolios.Any())
      {
        return NotFound("No portfolios found for the user.");
      }

      // Calculate statistics for each portfolio
      var portfolioStatisticsList = portfolios.Select(p => new PortfolioStatistic
      {
        Name = p.Name,
        TotalInvestment = (decimal)overallStats.TotalCustMarketValue,
        CurrentMarketValue = (decimal)overallStats.TotalMarketValue,
        TotalDifferenceValue = (decimal)overallStats.TotalDifferenceValue,
        TotalDividends = (decimal)overallStats.TotalDividends,
        TotalProfit = (decimal)overallStats.TotalPortfolioProfit,
        TotalDifferencePercentage = (decimal)overallStats.TotalDifferencePercentage,
        TotalProfitDifferencePercentage = (decimal)overallStats.TotalDifferenceWithDividendsPercentage,
      }).ToList();

      // Map to DTO for result
      var statistics = portfolios
          .Select(p => new PortfolioStatisticsDto
          {
            PortfolioName = p.Name,
            TotalValue = p.Items.Sum(i => i.Quantity * i.PurchasePrice),
            ItemCount = p.Items.Count
          })
          .ToList();

      var result = new
      {
        TotalValue = overallStats.TotalPortfolioProfit,
        Statistics = statistics
      };

      return Json(result);
    }

    public class PortfolioStatisticsDto
    {
      public string PortfolioName { get; set; }
      public decimal TotalValue { get; set; }
      public int ItemCount { get; set; }
    }


    [HttpGet("GetYearlyData")]
    public async Task<IActionResult> GetYearlyData1()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch the portfolios
      var portfolios = await _context.Portfolios
          .Where(p => p.UserId == userId)
          .Include(p => p.Items)
          .ToListAsync();

      // Get total revenue and investment data grouped by year
      var totalRevenueByYear = portfolios
          .SelectMany(p => p.Items)
          .GroupBy(item => item.PurchaseDate.Year)
          .Select(g => new
          {
            Year = g.Key,
            TotalRevenue = g.Sum(item => item.CurrentMarketValue),
            TotalInvestment = g.Sum(item => item.TotalInvestment)
          })
          .ToList();

      // Prepare data for the chart
      var series2021 = totalRevenueByYear
          .Where(x => x.Year == 2021)
          .Select(x => x.TotalRevenue)
          .ToList();

      var series2020 = totalRevenueByYear
          .Where(x => x.Year == 2020)
          .Select(x => x.TotalRevenue)
          .ToList();

      return Ok(new { series2021, series2020 });
    }
   

    [HttpGet("year-heatmap/{year}")]
    public async Task<IActionResult> GetHeatmapByYear(int year)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch the portfolios
      var portfolios = await _context.Portfolios
          .Where(p => p.UserId == userId)
          .Include(p => p.Items)
          .ToListAsync();

      // Fetch real-time prices for each item and update the item price
      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          var price = await _yahooFinanceService.GetRealTimePriceAsync(item.Symbol);
          item.CurrentPrice = (decimal)price.CurrentPrice;
        }
      }

      // Get total revenue and investment data grouped by month and symbol for the specified year
      var totalRevenueByMonthAndSymbol = portfolios
          .SelectMany(p => p.Items)
          .Where(item => item.PurchaseDate.Year == year)
          .GroupBy(item => new { item.PurchaseDate.Month, item.Symbol })
          .Select(g => new
          {
            Month = g.Key.Month,
            Symbol = g.Key.Symbol,
            TotalRevenue = g.Sum(item => item.CurrentPrice * item.Quantity),
            TotalInvestment = g.Sum(item => item.TotalInvestment),
            Difference = g.Sum(item => item.CurrentPrice * item.Quantity) - g.Sum(item => item.TotalInvestment)
          })
          .OrderBy(x => x.Month)
          .ThenBy(x => x.Symbol)
          .ToList();

      // Prepare data for the chart
      var seriesRevenue = new decimal[12];
      var seriesInvestment = new decimal[12];
      var seriesDifference = new decimal[12];

      foreach (var item in totalRevenueByMonthAndSymbol)
      {
        seriesRevenue[item.Month - 1] += item.TotalRevenue;
        seriesInvestment[item.Month - 1] += item.TotalInvestment;
        seriesDifference[item.Month - 1] += item.Difference;
      }

      // Include Symbol data in the response
      var symbolData = totalRevenueByMonthAndSymbol
          .GroupBy(x => x.Symbol)
          .Select(g => new
          {
            Symbol = g.Key,
            Data = g.Select(x => new
            {
              x.Month,
              x.TotalRevenue,
              x.TotalInvestment,
              x.Difference
            }).OrderBy(x => x.Month).ToList()
          })
          .ToList();

      return Ok(new { seriesRevenue, seriesInvestment, seriesDifference, symbolData });
    }

    [HttpGet("GetYearlyData/{year}")]
    public async Task<IActionResult> GetYearlyData(int year)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch the portfolios
      var portfolios = await _context.Portfolios
          .Where(p => p.UserId == userId)
          .Include(p => p.Items)
          .ToListAsync();

      // Fetch real-time prices for each item and update the item price
      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          var price =  _yahooFinanceService.GetRealTimePriceAsync(item.Symbol).Result.CurrentPrice;
          item.CurrentPrice = (decimal)price;
        }
      }

      // Get total revenue and investment data grouped by month for the specified year
      var totalRevenueByMonth = portfolios
          .SelectMany(p => p.Items)
          .Where(item => item.PurchaseDate.Year == year)
          .GroupBy(item => item.PurchaseDate.Month)
          .Select(g => new
          {
            Month = g.Key,
            TotalRevenue = g.Sum(item => item.CurrentPrice * item.Quantity),
            TotalInvestment = g.Sum(item => item.TotalInvestment),
            Difference = g.Sum(item => item.CurrentPrice * item.Quantity) - g.Sum(item => item.TotalInvestment)
          })
          .OrderBy(x => x.Month)
          .ToList();

      // Prepare data for the chart
      var seriesRevenue = new decimal[12];
      var seriesInvestment = new decimal[12];
      var seriesDifference = new decimal[12];

      foreach (var item in totalRevenueByMonth)
      {
        seriesRevenue[item.Month - 1] = item.TotalRevenue;
        seriesInvestment[item.Month - 1] = item.TotalInvestment;
        seriesDifference[item.Month - 1] = item.Difference;
      }

      return Ok(new { seriesRevenue, seriesInvestment, seriesDifference });
    }
    public async Task<IActionResult> Index()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      var portfolios = await _context.Portfolios
          .Where(p => p.UserId == userId)
          .Include(p => p.Items)
          .ToListAsync();

      if (portfolios == null || !portfolios.Any())
      {
        
        return View(null);
      }

      var portfolioPercentageResponses = await _portfolioService.CalculatePortfolioPercentagesAsync(portfolios);
      var totalRevenueByYear = _portfolioService.GetTotalRevenueByYear(portfolios);
      var totalRevenueByMonth = _portfolioService.GetTotalRevenueByMonth(portfolios);
      var (currentMonthRevenue, previousMonthRevenue) = _portfolioService.GetMonthRevenues(totalRevenueByMonth);
      var portfolioGrowthPercentage = await _portfolioService.CalculatePortfolioGrowthPercentage(userId);
      var dashboardData = await _portfolioService.GetDashboardDataAsync(userId);

      dashboardData.DividendsPercentage = portfolioPercentageResponses.Sum(s => s?.TotalDividendsPercentage);

      var amountTotalYearByItemsTask = Task.Run(() => _portfolioService.GetAmountTotalYearByItems(portfolios));
      var profileReportCurrentYearTask = Task.Run(() => _portfolioService.GetProfileReportCurrentYear(portfolios));
      var transactionsTask = Task.Run(() => _portfolioService.GetRecentTransactions(portfolios));

      var symbols = portfolios.SelectMany(p => p.Items).Select(i => i.Symbol).Distinct().ToList();
      var realTimeDataTask = FetchRealTimeData(symbols);

      await Task.WhenAll(
          amountTotalYearByItemsTask,
          profileReportCurrentYearTask,
          transactionsTask,
          realTimeDataTask
      );

      var model = new DashboardViewModel
      {
        Transactions = transactionsTask.Result,
        PortfolioGrowthPercentage = portfolioGrowthPercentage,
        TotalRevenueByYear = totalRevenueByYear,
        DashboardData = dashboardData,
        AmountTotalYear = amountTotalYearByItemsTask.Result,
        ProfileReportCurrentYear = profileReportCurrentYearTask.Result,
        RealTimeData = realTimeDataTask.Result,
      };

      return View(model);
    }

    public async Task<IActionResult> IndexOld()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch the portfolios first
      var portfolios = await _context.Portfolios
          .Where(p => p.UserId == userId)
          .Include(p => p.Items) // Include the related items
          .ToListAsync();

      // Get Total Revenue by year based on PortfolioItems purchase dates
      var totalRevenueByYear = portfolios
          .SelectMany(p => p.Items)
          .GroupBy(item => item.PurchaseDate.Year)
          .Select(g => new TotalRevenueByYearDto
          {
            Year = g.Key,
            TotalRevenue = g.Sum(item => item.CurrentMarketValue)
          })
          .ToList();

      // Get Portfolio Growth
      var dashboardData = await _portfolioService.GetDashboardDataAsync(userId);

      // Get Amount Total Year by each PortfolioItem
      var amountTotalYearByItems = portfolios
          .SelectMany(p => p.Items)
          .GroupBy(item => item.PurchaseDate.Year)
          .Select(g => new AmountTotalYearDto
          {
            Year = g.Key,
            TotalInvestment = g.Sum(item => item.TotalInvestment)
          })
          .ToList();

      // Get Profile Report current Year
      var currentYear = DateTime.Now.Year;
      var profileReportCurrentYear = portfolios
          .Where(p => p.CreationDate.HasValue && p.CreationDate.Value.Year == currentYear)
          .ToList();

      // Calculate Portfolio Growth Percentage
      var previousYear = currentYear - 1;
      var currentYearRevenue = totalRevenueByYear.FirstOrDefault(x => x.Year == currentYear)?.TotalRevenue ?? 0;
      var previousYearRevenue = totalRevenueByYear.FirstOrDefault(x => x.Year == previousYear)?.TotalRevenue ?? 0;

      decimal portfolioGrowthPercentage = 0;
      if (previousYearRevenue > 0)
      {
        portfolioGrowthPercentage = ((currentYearRevenue - previousYearRevenue) / previousYearRevenue) * 100;
      }
      // Get Transactions from PortfolioItems
      var fromDate = DateTime.Now.AddDays(-28);
      var transactions = portfolios
          .SelectMany(p => p.Items)
           .Where(item => item.PurchaseDate >= fromDate)
          .Select(item => new TransactionDto
          {
            Type = item.OperationType,
            Description = item.Symbol,
            Icon = PortfolioHelpers.GetIconForTransaction(item.Symbol),
            Amount = item.Quantity * item.PurchasePrice,
            Currency = "€",
            Date = item.PurchaseDate,
            Source = item.Symbol
          })
          .ToList();
      var symbols = portfolios.SelectMany(p => p.Items).Select(i => i.Symbol).Distinct().ToList();
      var realTimeData = await FetchRealTimeData(symbols);
      var model = new DashboardViewModel
      {
        Transactions = transactions,
        PortfolioGrowthPercentage = portfolioGrowthPercentage,
        TotalRevenueByYear = totalRevenueByYear,
        DashboardData = dashboardData,
        AmountTotalYear = amountTotalYearByItems,
        ProfileReportCurrentYear = profileReportCurrentYear,
        RealTimeData = realTimeData
      };
     
      return View(model);
    }
   

    private async Task<Dictionary<string, RealTimeDataDto>> FetchRealTimeData(List<string> symbols)
    {
      var realTimeData = new Dictionary<string, RealTimeDataDto>();

      foreach (var symbol in symbols?.Distinct())
      {
        try
        {
          var stockData = await _yahooFinanceService.GetRealTimePriceAsync(symbol);

          if (stockData != null)
          {
            realTimeData[symbol] = new RealTimeDataDto
            {
              Symbol = symbol,
              CurrentPrice = (decimal)stockData.CurrentPrice,
              Change = (decimal)stockData.Change,
              PercentChange = (decimal)stockData.PercentChange
            };
          }
        }
        catch (Exception ex)
        {
          // Log the exception (optional)
          //_logger.LogError(ex, $"Failed to get real-time data for symbol: {symbol}");
          // You can choose to add some default value or continue without adding to the dictionary
          realTimeData[symbol] = new RealTimeDataDto
          {
            Symbol = symbol,
            CurrentPrice = 0,
            Change = 0,
            PercentChange = 0
          };
        }
      }

      return realTimeData;
    }

   

  }

}
