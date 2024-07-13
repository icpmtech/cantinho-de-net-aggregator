using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services.News;
using AspnetCoreMvcFull.Models.News;
using AspnetCoreMvcFull.Controllers.api;
using Microsoft.Extensions.Logging;
using AspnetCoreMvcFull.Models.SetupDb;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers;
public class APilotController : Controller
{
  private readonly AppNewsService _newsService;
  private readonly ILogger<APilotController> _logger;
  public APilotController(AppNewsService newsService ,ILogger<APilotController> logger)
  {
    _newsService = newsService;
    _logger = logger;
  }
  public IActionResult Blank() => View();
  public IActionResult Container() => View();
  public IActionResult Fluid() => View();
  public  async Task<IActionResult> News(string category, string sortOrder, int pageNumber = 1, int pageSize = 50, string searchQuery = "") {

    var paginatedNews = await _newsService.GetPaginatedNewsAsync(category, sortOrder, pageNumber, pageSize, searchQuery);
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
