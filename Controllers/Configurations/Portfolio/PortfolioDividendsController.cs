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
 
  public class PortfolioDividendsController : Controller
  {
    private readonly PortfolioService _portfolioService;
    private readonly NewsService _newsService;
    private readonly ILogger<PortfolioDividendsController> _logger;
    private readonly ApplicationDbContext _context;

    public PortfolioDividendsController(ApplicationDbContext context, PortfolioService portfolioService, NewsService newsService, ILogger<PortfolioDividendsController> logger)
    {
      _newsService = newsService;
      _portfolioService = portfolioService;
      _logger = logger;
      _context = context;
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

