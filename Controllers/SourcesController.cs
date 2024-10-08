using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services.News;
using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Controllers.api;
using Microsoft.Extensions.Logging;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Controllers;
public class SourcesController : Controller
{
  private readonly AppNewsService _newsService;
  private readonly ILogger<SourcesController> _logger;
  public SourcesController(AppNewsService newsService ,ILogger<SourcesController> logger)
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
    ViewBag.PageSize = pageSize; // Pass pageSize to view
    ViewBag.SearchQuery = searchQuery; // Pass searchQuery to view
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
