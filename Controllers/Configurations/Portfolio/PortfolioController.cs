using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Controllers.Configurations.Portfolio;
using System.Security.Claims;

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


    public IActionResult SummaryXtb()
    {

      return View();
    }
    public IActionResult Transactions()
    {

      return View();
    }
    public IActionResult TransactionsPivot()
    {

      return View();
    }
    public IActionResult Holdings()
    {

      return View();
    }
    public IActionResult Summary()
    {

      return View();
    }

    public IActionResult Performance()
    {

      return View();
    }
    // Partial view for the Highlights section
    public ActionResult Highlights()
    {
      return PartialView(); // Renders the Highlights section
    }

    // Partial view for the Portfolio Evolution (Line Chart)
    public ActionResult PortfolioEvolution()
    {
      return PartialView(); // Renders the Portfolio Evolution chart
    }

    // Partial view for the Radar Chart
    public ActionResult RadarChart()
    {
      return PartialView(); // Renders the Radar Chart
    }

    // Partial view for Monthly Stock Returns (Bar Chart)
    
    public ActionResult MonthlyStockReturns()
    {
      return PartialView(); // Renders the Bar Chart
    }

    // Partial view for Portfolio Allocation (Pie Chart)
    public ActionResult PortfolioAllocation()
    {
      return PartialView(); // Renders the Pie Chart
    }

    // Partial view for Event Sentiments (Events Chart)
    public ActionResult EventSentiments()
    {
      return PartialView(); // Renders the Events Sentiment chart
    }
    public IActionResult PortfolioTransactionsV1()
    {

      return View();
    }
    public IActionResult LossesManager()
    {

      return View();
    }
    
       public IActionResult AnalyticsV1()
    {

      return View();
    }
    public IActionResult Index()
    {

      return View();
    }
    public IActionResult PortfolioOperations()
    {

      return View();
    }
    public IActionResult PortfolioSummary()
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
    public async Task<IActionResult> PortfolioUpcomingDividends()
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
    public async Task<IActionResult> PortfolioAllDividends()
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
    
    public IActionResult DividendsCalendar()
    {
      return View();
    }





  }

 

}

