using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.Configurations.Linkedin
{
  public class LinkedinController : Controller
  {
    private readonly NewsService _newsService;
    private readonly ILogger<LinkedinController> _logger;
    private readonly ApplicationDbContext _context;

    public LinkedinController(ApplicationDbContext context, NewsService newsService, ILogger<LinkedinController> logger)
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
