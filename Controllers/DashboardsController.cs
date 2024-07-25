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

namespace MarketAnalyticHub.Controllers
{
  [Authorize]
  public class DashboardsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly PortfolioService _portfolioService;

    public DashboardsController(ApplicationDbContext context, PortfolioService portfolioService)
    {
      _context = context;
      _portfolioService = portfolioService;
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

        // Get total revenue and investment data grouped by month for the specified year
        var totalRevenueByMonth = portfolios
            .SelectMany(p => p.Items)
            .Where(item => item.PurchaseDate.Year == year)
            .GroupBy(item => item.PurchaseDate.Month)
            .Select(g => new
            {
              Month = g.Key,
              TotalRevenue = g.Sum(item => item.CurrentMarketValue),
              TotalInvestment = g.Sum(item => item.TotalInvestment)
            })
            .OrderBy(x => x.Month)
            .ToList();

        // Prepare data for the chart
        var seriesRevenue = new decimal[12];
        var seriesInvestment = new decimal[12];

        foreach (var item in totalRevenueByMonth)
        {
          seriesRevenue[item.Month - 1] = item.TotalRevenue;
          seriesInvestment[item.Month - 1] = item.TotalInvestment;
        }

        return Ok(new { seriesRevenue, seriesInvestment });
      }
    
    public async Task<IActionResult> Index()
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

      var model = new DashboardViewModel
      {
        PortfolioGrowthPercentage = portfolioGrowthPercentage,
        TotalRevenueByYear = totalRevenueByYear,
        DashboardData = dashboardData,
        AmountTotalYear = amountTotalYearByItems,
        ProfileReportCurrentYear = profileReportCurrentYear
      };

      return View(model);
    }


  }

}
