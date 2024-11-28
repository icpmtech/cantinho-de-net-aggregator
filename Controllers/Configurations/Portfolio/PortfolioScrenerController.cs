using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;

using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Controllers.Configurations.Reddit
{

  public class PortfolioScrenerController : Controller
  {
    private readonly PortfolioService _portfolioService;
    private readonly NewsService _newsService;
    private readonly ILogger<PortfolioScrenerController> _logger;
    private readonly ApplicationDbContext _context;

    private readonly IYahooFinanceService _yahooFinanceService;

    public PortfolioScrenerController(IYahooFinanceService yahooFinanceService, ApplicationDbContext context, PortfolioService portfolioService, NewsService newsService, ILogger<PortfolioScrenerController> logger)
    {
      _newsService = newsService;
      _portfolioService = portfolioService;
      _logger = logger;
      _context = context;
      _yahooFinanceService = yahooFinanceService;
    }


    // GET: PortfolioScrener
    public async Task<ActionResult> Index(string stockSymbol, string companyName)
    {
      var viewModel = new ScreenerViewModel
      {
        HasQuery = !string.IsNullOrWhiteSpace(stockSymbol) || !string.IsNullOrWhiteSpace(companyName),
        Stocks = new List<StockViewModel>()
      };

      if (viewModel.HasQuery)
      {
        var stock = await _yahooFinanceService.GetStockDataAsync(stockSymbol);
        var summary=await _yahooFinanceService.GetSummaryBySymbolAsync(stockSymbol);
       
        if (stock != null&& summary != null)
        {
          stock.Industry = summary.Industry;
          stock.CEO = summary.CEO;
          stock.Sector = summary.Sector;
          stock.Description = summary.Description;
          // Fetch historical data
          stock.ChartData = await _yahooFinanceService.GetHistoricalDataAsync(stockSymbol, DateTime.Today.AddMonths(-1), DateTime.Today);

          // Mock News and Sentiment (optional)
          stock.News = await _yahooFinanceService.GetMockNews(stockSymbol); // Replace with actual implementation if needed
          stock.SentimentScore = GetMockSentiment(stockSymbol); // Replace with actual implementation if needed

          viewModel.Stocks.Add(stock);
        }
      }

      return View(viewModel);
    }
    // GET: PortfolioScrener
    public async Task<ActionResult> Details(string stockSymbol, string companyName)
    {
      var viewModel = new ScreenerViewModel
      {
        HasQuery = !string.IsNullOrWhiteSpace(stockSymbol) || !string.IsNullOrWhiteSpace(companyName),
        Stocks = new List<StockViewModel>()
      };

      if (viewModel.HasQuery)
      {
        var stock = await _yahooFinanceService.GetStockDataAsync(stockSymbol);
        var summary = await _yahooFinanceService.GetSummaryBySymbolAsync(stockSymbol);

        if (stock != null && summary != null)
        {
          stock.Industry = summary.Industry;
          stock.CEO = summary.CEO;
          stock.Sector = summary.Sector;
          stock.Description = summary.Description;
          // Fetch historical data
          stock.ChartData = await _yahooFinanceService.GetHistoricalDataAsync(stockSymbol, DateTime.Today.AddMonths(-1), DateTime.Today);

          // Mock News and Sentiment (optional)
          stock.News = await _yahooFinanceService.GetMockNews(stockSymbol); // Replace with actual implementation if needed
          stock.SentimentScore = GetMockSentiment(stockSymbol); // Replace with actual implementation if needed

          viewModel.Stocks.Add(stock);
        }
      }

      return View(viewModel);
    }


    private double GetMockSentiment(string symbol)
    {
      // Mock sentiment score between -1 (negative) and 1 (positive)
      var sentiments = new Dictionary<string, double>
            {
                { "AAPL", 0.8 },
                { "MSFT", 0.6 },
                { "GOOGL", 0.7 }
                // Add more mappings as needed
            };

      return sentiments.ContainsKey(symbol) ? sentiments[symbol] : 0.0;
    }
  }
}

