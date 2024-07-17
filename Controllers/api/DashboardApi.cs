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
using AspnetCoreMvcFull.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class DashboardsController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public DashboardsController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet("data")]
    public async Task<ActionResult<DashboardData>> GetDashboardData()
    {
      // Replace "userId" with the actual user ID from your authentication context
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

      var data = await GetDashboardDataAsync(userId);
      return Ok(data);
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
    public async Task<IActionResult> GetExpensesData()
    {
      // Replace the hardcoded data with actual data retrieval logic
      var expensesData = new
      {
        Series = 65 // Example data
      };

      return Ok(expensesData);
    }


    // GET: api/Dashboards/income
    [HttpGet("income")]
    public async Task<IActionResult> GetIncomeData()
    {
      // Replace the hardcoded data with actual data retrieval logic
      var incomeData = new
      {
        Series = new[] { 24, 21, 30, 22, 42, 26, 35, 29 },
        Categories = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul" }
      };

      return Ok(incomeData);
    }
    // GET: api/Dashboards/profit
    [HttpGet("profit")]
    public async Task<IActionResult> GetProfitData()
    {
      // Replace the hardcoded data with actual data retrieval logic
      var profitData = new
      {
        Data = new[] { 110, 270, 145, 245, 205, 285 }
      };

      return Ok(profitData);
    }
    // GET: api/Dashboards/order-statistics
    [HttpGet("symbol-statistics")]
    public async Task<IActionResult> GetSymbolsStatistics()
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
      var growthData = new
      {
        Growth = 78
      };

      return Ok(growthData);
    }

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
      var portfolios = await _context.Portfolios
                                        .Include(p => p.Items)
                                        .ThenInclude(pi => pi.Dividends)
                                        .Where(p => p.UserId == userId)
                                        .ToListAsync();

      var portfolioStatistics = portfolios.Select(p => new PortfolioStatistic
      {
        Name = p.Name,
        TotalInvestment = p.TotalInvestment,
        CurrentMarketValue = p.CurrentMarketValue,
        Items = p.Items.Select(i => new PortfolioItemDetail
        {
          Symbol = i.Symbol,
          TotalInvestment = i.TotalInvestment,
          CurrentMarketValue = i.CurrentMarketValue,
          Dividends = i.Dividends.Sum(d => d.Amount)
        }).ToList()
      });

      return Ok(portfolioStatistics);
    }

    private async Task<DashboardData> GetDashboardDataAsync(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);

      decimal totalInvestment = portfolios.Sum(p => p.TotalInvestment);
      decimal currentMarketValue = portfolios.Sum(p => p.CurrentMarketValue);
      decimal dividends = portfolios.Sum(p => p.Items.Sum(i => i.Dividends.Sum(d => d.Amount)));
      decimal profit = currentMarketValue - totalInvestment;
      decimal payments = 2456; // This should be replaced with actual payments data
      decimal operations = 14857; // This should be replaced with actual operations data
      decimal yearlyReport = 84686; // This should be replaced with actual yearly report data
      decimal growth = totalInvestment > 0 ? (profit / totalInvestment) * 100 : 0;
      decimal portfolioGrowth = totalInvestment > 0 ? (currentMarketValue / totalInvestment) * 100 : 0;

      return new DashboardData
      {
        Profit = profit,
        Dividends = dividends,
        Payments = payments,
        Operations = operations,
        TotalRevenue = currentMarketValue, // Example data
        Growth = growth,
        PortfolioGrowth = portfolioGrowth,
        YearlyReport = yearlyReport
      };
    }
  }


}