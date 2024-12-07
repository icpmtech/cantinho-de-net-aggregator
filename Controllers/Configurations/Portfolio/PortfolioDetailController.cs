using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Controllers.Configurations.Portfolio;
using MarketAnalyticHub.Models.Portfolio;
using System.Security.Claims;
using MarketAnalyticHub.Models.ViewsModels;
using MarketAnalyticHub.Controllers.api;

namespace MarketAnalyticHub.Controllers.Configurations.Portfolio
{

  public partial class PortfolioDetailController : Controller
  {
    private readonly PortfolioService _portfolioService;
    private readonly NewsService _newsService;
    private readonly ILogger<PortfolioController> _logger;
    private readonly ApplicationDbContext _context;

    public PortfolioDetailController(ApplicationDbContext context, PortfolioService portfolioService, NewsService newsService, ILogger<PortfolioController> logger)
    {
      _newsService = newsService;
      _portfolioService = portfolioService;
      _logger = logger;
      _context = context;
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
    public IActionResult LossesManager()
    {

      return View();
    }
 
       public async Task<IActionResult> Index(string stockSymbol)
    {
      if (string.IsNullOrWhiteSpace(stockSymbol))
      {
        // Handle invalid stock symbol
        TempData["Error"] = "Invalid stock symbol provided.";
        return RedirectToAction("Error", "Home");
      }

      try
      {
        // Fetch current price and basic information
        var dataDynamic = await YahooService.GetCurrentPriceAsync(stockSymbol);

        // Check if dataDynamic is null
        if (dataDynamic == null)
        {
          TempData["Error"] = "No data found for the provided stock symbol.";
          return RedirectToAction("Error", "Home");
        }

        // Determine if dataDynamic is a collection or a single object
        IEnumerable<dynamic> dynamicDataCollection;
        dynamic firstItem;

        if (dataDynamic is IEnumerable<dynamic> dynamicEnumerable && dynamicEnumerable.Any())
        {
          dynamicDataCollection = dynamicEnumerable;
          firstItem = dynamicEnumerable.First();
        }
        else
        {
          // If it's a single object, wrap it in a collection for consistency
          dynamicDataCollection = new List<dynamic> { dataDynamic };
          firstItem = dataDynamic;
        }

        // Initialize the model with default values
        var model = new PortfolioDetailViewModel
        {
          Symbol = stockSymbol
        };

        // Map dynamic data to model properties with null checks
        model.ShortName = firstItem?.shortName ?? "N/A";
        model.LongName = firstItem?.longName ?? "N/A";
        model.RegularMarketPrice = firstItem?.regularMarketPrice ?? 0;
        model.RegularMarketChange = firstItem?.regularMarketChange ?? 0;
        model.RegularMarketChangePercent = firstItem?.regularMarketChangePercent ?? 0;
        model.MarketCap = firstItem?.marketCap ?? 0;
        model.TrailingPE = firstItem?.trailingPE ?? 0;
        model.ForwardPE = firstItem?.forwardPE ?? 0;
        //model.DividendYield = firstItem?.dividendYield ?? 0;
        model.FiftyTwoWeekRange = firstItem?.fiftyTwoWeekRange ?? "N/A";

        // Fetch additional data if available
        // For example: Financials, Historical Data, News, Company Overview

        // Example: Fetch Financials
        //var financials = await YahooService.GetFinancialsAsync(stockSymbol);
        //if (financials != null)
        //{
        //  model.Revenue = financials.Revenue ?? 0m;
        //  model.NetIncome = financials.NetIncome ?? 0m;
        //  model.EBITDA = financials.EBITDA ?? 0m;
        //}

        //// Example: Fetch Historical Data
        //var historicalData = await YahooService.GetHistoricalDataAsync(stockSymbol);
        //if (historicalData != null && historicalData.Any())
        //{
        //  model.HistoricalData = historicalData.Select(d => new ChartDataPortfolioDetailViewModel
        //  {
        //    Date = d.Date,
        //    Value = d.Value
        //  }).ToList();
        //}

        //// Example: Fetch Recent News
        //var recentNews = await YahooService.GetRecentNewsAsync(stockSymbol);
        //if (recentNews != null && recentNews.Any())
        //{
        //  model.RecentNews = recentNews.Select(n => new NewsItem
        //  {
        //    Date = n.Date,
        //    Title = n.Title
        //  }).ToList();
        //}

        //// Example: Fetch Company Overview
        //var overview = await YahooService.GetCompanyOverviewAsync(stockSymbol);
        //model.CompanyOverview = !string.IsNullOrEmpty(overview) ? overview : "No company overview available.";

        // Pass the populated model to the view
        return View(model);
      }
      catch (Exception ex)
      {
        // Log the exception (implement logging as per your project's standards)
        // For demonstration, we'll use TempData to pass the error message
        TempData["Error"] = $"An error occurred while fetching stock data: {ex.Message}";
        return RedirectToAction("Error", "Home");
      }
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

