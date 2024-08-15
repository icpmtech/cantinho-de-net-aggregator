using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Controllers.Configurations.Portfolio;
using MarketAnalyticHub.Models.Portfolio;
using System.Security.Claims;
using DocumentFormat.OpenXml.Wordprocessing;

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


    
       public IActionResult LossesManager()
    {

      return View();
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
    public async Task<IActionResult> Fundamentals()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }


      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
      var model = new PortfolioListViewModel
      {
        Portfolios = portfolios
      };

      return View(model);
    }

    public IActionResult Qualitatives()
    {

      return View();
    }
    public IActionResult Dividends()
    {
      return View();
    }
    public IActionResult DividendsCalendar()
    {
      return View();
    }





  }

 

}

