using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.Configurations.Reddit
{
  public class PortfolioController : Controller
  {
    private readonly NewsService _newsService;
    private readonly ILogger<PortfolioController> _logger;
    private readonly ApplicationDbContext _context;

    public PortfolioController(ApplicationDbContext context, NewsService newsService, ILogger<PortfolioController> logger)
    {
      _newsService = newsService;
      _logger = logger;
      _context = context;
    }
    public IActionResult Index()
    {
      var news = _context.NewsScrapingItem.ToList();

      return View(news);
    }

    public IActionResult Fundamentals()
    {
      var news = _context.NewsScrapingItem.ToList();

      return View(news);
    }

    public IActionResult Qualitatives()
    {
      var news = _context.NewsScrapingItem.ToList();

      return View(news);
    }
  }
}

