using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.Configurations.Reddit
{
  public class RedditController : Controller
  {
    private readonly NewsService _newsService;
    private readonly ILogger<RedditController> _logger;
    private readonly ApplicationDbContext _context;

    public RedditController(ApplicationDbContext context, NewsService newsService, ILogger<RedditController> logger)
    {
      _newsService = newsService;
      _logger = logger;
      _context = context;
    }
    public IActionResult Index()
    {
      var news = _context.NewsScrapingItems.ToList();

      return View(news);
    }
  }
}
