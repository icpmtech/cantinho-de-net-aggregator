using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services.News;
using MarketAnalyticHub.Models.News;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MarketAnalyticHub.Controllers
{
  public class APilotController : Controller
  {
    private readonly AppNewsService _newsService;
    private readonly ILogger<APilotController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly PortfolioService _portfolioService;
    public APilotController(AppNewsService newsService, ILogger<APilotController> logger, ApplicationDbContext context, PortfolioService portfolioService)
    {
      _context = context;
      _portfolioService = portfolioService;
      _newsService = newsService;
      _logger = logger;
    }

    public IActionResult Blank() => View();
    public IActionResult Container() => View();
    public IActionResult Fluid() => View();

    public async Task<IActionResult> News(string category, string sortOrder, int pageNumber = 1, int pageSize = 50, string searchQuery = "", DateTime? startDate = null, DateTime? endDate = null)
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
      var symbols = portfolios.SelectMany(p => p.Items.Select(pi => pi.Symbol)).ToList();

      var paginatedNews = await _newsService.GetPaginatedNewsAsync(category, sortOrder, pageNumber, pageSize, searchQuery, startDate, endDate);
      // Filter the news items to only include those with tickers in the user's portfolios
      var filteredNews = paginatedNews
          .Where(n => n.Tickers.Any(ticker => symbols.Contains(ticker)))
          .ToList();
      
      ViewBag.PageSize = pageSize; // Pass pageSize to view
      ViewBag.SearchQuery = searchQuery; // Pass searchQuery to view
      ViewBag.Category = category; // Pass category to view
      ViewBag.SortOrder = sortOrder; // Pass sortOrder to view
      ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd"); // Pass startDate to view
      ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd"); // Pass endDate to view

      var positiveData = paginatedNews
          .Where(item => item.Sentiment > 0)
          .GroupBy(item => item.Category)
          .Select(g => new { Symbol = g.Key, Count = g.Count() })
          .ToList();

      var negativeData = paginatedNews
          .Where(item => item.Sentiment < 0)
          .GroupBy(item => item.Category)
          .Select(g => new { Symbol = g.Key, Count = g.Count() })
          .ToList();
      
      ViewBag.PositiveDataJson = System.Text.Json.JsonSerializer.Serialize(positiveData);
      ViewBag.NegativeDataJson = System.Text.Json.JsonSerializer.Serialize(negativeData);
      return View(paginatedNews);
    }
    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetNewsItem(int id)
    {
      var newsItem = await _newsService.GetNewsByIdAsync(id);
      if (newsItem == null)
      {
        return NotFound();
      }
      return Json(newsItem);
    }

    public IActionResult Reddit() => View();
    public IActionResult Linkedin() => View();
    public IActionResult Facebook() => View();
    public IActionResult HorizontalMenu() => View();
    public IActionResult WithoutMenu() => View();
    public IActionResult WithoutNavbar() => View();
  }
}
