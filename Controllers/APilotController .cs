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

namespace MarketAnalyticHub.Controllers
{
  public class APilotController : Controller
  {
    private readonly AppNewsService _newsService;
    private readonly ILogger<APilotController> _logger;

    public APilotController(AppNewsService newsService, ILogger<APilotController> logger)
    {
      _newsService = newsService;
      _logger = logger;
    }

    public IActionResult Blank() => View();
    public IActionResult Container() => View();
    public IActionResult Fluid() => View();

    public async Task<IActionResult> News(string category, string sortOrder, int pageNumber = 1, int pageSize = 50, string searchQuery = "", DateTime? startDate = null, DateTime? endDate = null)
    {
      var paginatedNews = await _newsService.GetPaginatedNewsAsync(category, sortOrder, pageNumber, pageSize, searchQuery, startDate, endDate);
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

      ViewBag.PositiveDataJson = JsonSerializer.Serialize(positiveData);
      ViewBag.NegativeDataJson = JsonSerializer.Serialize(negativeData);
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
