using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.Configurations.Reddit
{
 
  public class PortfolioController : Controller
  {
    private readonly PortfolioService _portfolioService;
    private readonly NewsService _newsService;
    private readonly ILogger<PortfolioController> _logger;
    private readonly ApplicationDbContext _context;

    public PortfolioController(ApplicationDbContext context, PortfolioService portfolioService, NewsService newsService, ILogger<PortfolioController> logger)
    {
      _newsService = newsService;
      _portfolioService = portfolioService;
      _logger = logger;
      _context = context;
    }
    public IActionResult Index()
    {

      return View();
    }
    [HttpGet("GetPortfolioStatistics")]
    public IActionResult GetPortfolioStatistics()
    {
      var statistics = _portfolioService.GetPortfolioStatistics();
      return Ok(statistics);
    }
    public IActionResult Fundamentals()
    {

      return View();
    }

    public IActionResult Qualitatives()
    {

      return View();
    }
    public IActionResult Dividends()
    {
      return View();
    }
  }
}

