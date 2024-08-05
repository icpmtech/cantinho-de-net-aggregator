using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models.Configurations.News;
using MarketAnalyticHub.Controllers.api;
using Microsoft.Extensions.Logging;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models.News;
using ApplicationDbContext = MarketAnalyticHub.Models.SetupDb.ApplicationDbContext;
using Hangfire;
using MarketAnalyticHub.Services.Jobs;

namespace MarketAnalyticHub.Controllers;

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
    news.AuthorSelector = news.AuthorSelector ?? "EMPTY";
    news.TitleSelector = news.TitleSelector ?? "EMPTY";
    news.LinkSelector = news.LinkSelector ?? "EMPTY";
    news.DescriptionSelector = news.DescriptionSelector ?? "EMPTY";
    news.DateSelector = news.DateSelector ?? "EMPTY";
    if (ModelState.IsValid)
    {
      _context.NewsScrapingItem.Add(news);
      _context.SaveChanges();
      return Json(new { success = true, message = "News added successfully!" });
    }

    return Json(new { success = false, message = "Invalid data received.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
  }
  [HttpPost("runnews/{id}")]
  public IActionResult RunJob(int id)
  {
    using (var scope = HttpContext.RequestServices.CreateScope())
    {
      var serviceProvider = scope.ServiceProvider;
      var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
      var newsScraper = serviceProvider.GetRequiredService<NewsScraper>();

      try
      {
        // Schedule the scraping job to run immediately
        recurringJobManager.AddOrUpdate(
            "scrape-news-"+id,
            () => newsScraper.ScrapeNewsAsync(id), // Pass the id if necessary
            Cron.Never); // Run once immediately

        // Trigger the job manually for immediate execution
        BackgroundJob.Enqueue(() => newsScraper.ScrapeNewsAsync(id));

        return Json(new { success = true, message = "Run successfully!" });
      }
      catch (Exception ex)
      {
        // Handle any errors that occur during the job scheduling
        return Json(new { success = false, message = $"Error: {ex.Message}" });
      }
    }
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
    news.AuthorSelector = news.AuthorSelector ?? "EMPTY";
    news.TitleSelector = news.TitleSelector ?? "EMPTY";
    news.LinkSelector = news.LinkSelector ?? "EMPTY";
    news.DescriptionSelector = news.DescriptionSelector ?? "EMPTY";
    news.DateSelector = news.DateSelector ?? "EMPTY";
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
