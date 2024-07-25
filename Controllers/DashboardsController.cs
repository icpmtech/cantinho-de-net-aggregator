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

    public async Task<IActionResult> Index()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      // Fetch the portfolios first
      var portfolios = await _context.Portfolios
          .Where(p => p.UserId == userId && p.CreationDate.HasValue)
          .Include(p => p.Items) // Include the related items
          .ToListAsync();

      // Get Total Revenue by year
      var totalRevenueByYear = portfolios
          .Where(p => p.CreationDate.HasValue)
          .GroupBy(p => p.CreationDate.Value.Year)
          .Select(g => new TotalRevenueByYearDto
          {
            Year = g.Key,
            TotalRevenue = g.Sum(p => p.CurrentMarketValue)
          })
          .ToList();

      // Get Portfolio Growth
      var dashboardData = await _portfolioService.GetDashboardDataAsync(userId);

      // Get Amount Total Year
      var amountTotalYear = portfolios
          .Where(p => p.CreationDate.HasValue)
          .GroupBy(p => p.CreationDate.Value.Year)
          .Select(g => new AmountTotalYearDto
          {
            Year = g.Key,
            TotalInvestment = g.Sum(p => p.TotalInvestment)
          })
          .ToList();

      // Get Profile Report current Year
      var currentYear = DateTime.Now.Year;
      var profileReportCurrentYear = portfolios
          .Where(p => p.CreationDate.HasValue && p.CreationDate.Value.Year == currentYear)
          .ToList();

      var model = new DashboardViewModel
      {
        TotalRevenueByYear = totalRevenueByYear,
        DashboardData = dashboardData,
        AmountTotalYear = amountTotalYear,
        ProfileReportCurrentYear = profileReportCurrentYear
      };

      return View(model);
    }
  }

 }
