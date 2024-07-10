using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Models.News;
using AspnetCoreMvcFull.Controllers.api;
using Microsoft.Extensions.Logging;

namespace AspnetCoreMvcFull.Controllers;

public class SourcesController : Controller
{
  private readonly NewsService _newsService;
  private readonly ILogger<SourcesController> _logger;
  public SourcesController(NewsService newsService,ILogger<SourcesController> logger)
  {
    _newsService = newsService;
    _logger = logger;
  }
  public IActionResult Blank() => View();
  public IActionResult Container() => View();
  public IActionResult Fluid() => View();
  public  async Task<IActionResult> News(string category) {

    IEnumerable<NewsItem> news;
    try
    {
      news = await _newsService.GetNewsAsync();
      if (!string.IsNullOrEmpty(category))
      {
        news = news.Where(n => n.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
      }
    }
    catch (HttpRequestException ex)
    {
      _logger.LogError(ex, "Failed to fetch or parse news.");
      return StatusCode(500, "Failed to fetch or parse news.");
    }

    ViewBag.Categories = news.Select(n => n.Category).Distinct().ToList();
    ViewBag.SelectedCategory = category;

    return View(news);
  } 
  public IActionResult HorizontalMenu() => View();
  public IActionResult WithoutMenu() => View();
  public IActionResult WithoutNavbar() => View();
}
