using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Models.Configurations.News;
using AspnetCoreMvcFull.Controllers.api;
using Microsoft.Extensions.Logging;
using AspnetCoreMvcFull.Models.SetupDb;
using AspnetCoreMvcFull.Models.News;
using ApplicationDbContext = AspnetCoreMvcFull.Models.SetupDb.ApplicationDbContext;

namespace AspnetCoreMvcFull.Controllers;

public class ConfigurationsController : Controller
{
  private readonly NewsService _newsService;
  private readonly ILogger<SourcesController> _logger;
  private readonly ApplicationDbContext _context;

  public ConfigurationsController(ApplicationDbContext context,NewsService newsService,ILogger<SourcesController> logger)
  {
    _newsService = newsService;
    _logger = logger;
    _context = context;
  }
  public IActionResult Blank() => View();
  public IActionResult Container() => View();
  public IActionResult Fluid() => View();
  public IActionResult News()
  {

    var news= _context.NewsScrapingItem.ToList();

    return View(news);
  }
  [HttpPost("addnews")]
  public IActionResult Add([FromBody] NewsScrapingItem news)
  {
    if (ModelState.IsValid)
    {
      _context.NewsScrapingItem.Add(news);
      _context.SaveChanges();
      return Json(new { success = true, message = "News added successfully!" });
    }

    return Json(new { success = false, message = "Invalid data received.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
  }

  [HttpDelete("deletenews/{id}")]
  public IActionResult Delete(int id)
  {
    var newsItem = _context.NewsScrapingItem.Find(id);
    if (newsItem != null)
    {
      _context.NewsScrapingItem.Remove(newsItem);
      _context.SaveChanges();
      return Json(new { success = true, message = "News deleted successfully!" });
    }

    return Json(new { success = false, message = "News item not found." });
  }
  [HttpGet("getnews/{id}")]
  public IActionResult GetNews(int id)
  {
    var newsItem = _context.NewsScrapingItem.Find(id);
    if (newsItem != null)
    {
      return Json(new { success = true, news = newsItem });
    }

    return Json(new { success = false, message = "News item not found." });
  }

  [HttpPut("editnews/{id}")]
  public IActionResult EditNews(int id, [FromBody] NewsScrapingItem news)
  {
    if (id != news.Id)
    {
      return BadRequest(new { success = false, message = "ID mismatch." });
    }

    if (ModelState.IsValid)
    {
      _context.NewsScrapingItem.Update(news);
      _context.SaveChanges();
      return Ok(new { success = true, message = "News updated successfully!" });
    }
    return BadRequest(new { success = false, message = "Invalid data received.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
  }

  public IActionResult HorizontalMenu() => View();
  public IActionResult WithoutMenu() => View();
  public IActionResult WithoutNavbar() => View();
}
