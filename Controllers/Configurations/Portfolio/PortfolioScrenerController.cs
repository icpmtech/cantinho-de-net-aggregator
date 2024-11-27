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

    public PortfolioScrenerController(ApplicationDbContext context, PortfolioService portfolioService, NewsService newsService, ILogger<PortfolioScrenerController> logger)
    {
      _newsService = newsService;
      _portfolioService = portfolioService;
      _logger = logger;
      _context = context;
    }

    // GET: PortfolioScrener
    public ActionResult Index(string stockSymbol, string companyName)
    {
      var viewModel = new ScreenerViewModel
      {
        HasQuery = !string.IsNullOrWhiteSpace(stockSymbol) || !string.IsNullOrWhiteSpace(companyName),
        Stocks = new List<StockViewModel>()
      };

      if (viewModel.HasQuery)
      {
        // Retrieve all mock stocks
        var allStocks = GetAllStocks();

        // Filter based on search criteria
        var filteredStocks = allStocks.Where(s =>
            (string.IsNullOrWhiteSpace(stockSymbol) || s.Symbol.Contains(stockSymbol, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(companyName) || s.CompanyName.Contains(companyName, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        // Populate additional mock data for each stock
        foreach (var stock in filteredStocks)
        {
          PopulateAdditionalMockData(stock);
        }

        viewModel.Stocks = filteredStocks;
      }

      return View(viewModel);
    }

    private List<StockViewModel> GetAllStocks()
    {
      // Mock stock data
      return new List<StockViewModel>
            {
                new StockViewModel
                {
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Price = 150.00m,
                    Change = 1.25m,
                    MarketCap = 2500000000000
                },
                new StockViewModel
                {
                    Symbol = "MSFT",
                    CompanyName = "Microsoft Corporation",
                    Price = 280.00m,
                    Change = -0.85m,
                    MarketCap = 2100000000000
                },
                new StockViewModel
                {
                    Symbol = "GOOGL",
                    CompanyName = "Alphabet Inc.",
                    Price = 2700.00m,
                    Change = 0.50m,
                    MarketCap = 1800000000000
                },
                // Add more mock data as needed
            };
    }

    private void PopulateAdditionalMockData(StockViewModel stock)
    {
      // Existing Mock Company Details
      stock.Sector = GetMockSector(stock.Symbol);
      stock.Industry = GetMockIndustry(stock.Symbol);
      stock.Description = GetMockDescription(stock.Symbol);
      stock.CEO = GetMockCEO(stock.Symbol);

      // Existing Mock Chart Data
      stock.ChartData = GetMockChartData(stock.Symbol);

      // Existing Mock News Articles
      stock.News = GetMockNews(stock.Symbol);

      // Existing Mock Sentiment Score
      stock.SentimentScore = GetMockSentiment(stock.Symbol);

      // New Mock Financial Details
      stock.PERatio = GetMockPERatio(stock.Symbol);
      stock.EPS = GetMockEPS(stock.Symbol);
      stock.FiftyTwoWeekHigh = GetMockFiftyTwoWeekHigh(stock.Symbol);
      stock.FiftyTwoWeekLow = GetMockFiftyTwoWeekLow(stock.Symbol);
      stock.Volume = GetMockVolume(stock.Symbol);
      stock.DividendYield = GetMockDividendYield(stock.Symbol);
    }
    private double GetMockPERatio(string symbol)
    {
      var peRatios = new Dictionary<string, double>
    {
        { "AAPL", 28.5 },
        { "MSFT", 35.2 },
        { "GOOGL", 30.1 }
        // Add more mappings as needed
    };

      return peRatios.ContainsKey(symbol) ? peRatios[symbol] : 0.0;
    }

    private decimal GetMockEPS(string symbol)
    {
      var epsValues = new Dictionary<string, decimal>
    {
        { "AAPL", 5.11m },
        { "MSFT", 8.05m },
        { "GOOGL", 6.24m }
        // Add more mappings as needed
    };

      return epsValues.ContainsKey(symbol) ? epsValues[symbol] : 0.0m;
    }

    private decimal GetMockFiftyTwoWeekHigh(string symbol)
    {
      var highs = new Dictionary<string, decimal>
    {
        { "AAPL", 180.00m },
        { "MSFT", 310.00m },
        { "GOOGL", 3000.00m }
        // Add more mappings as needed
    };

      return highs.ContainsKey(symbol) ? highs[symbol] : 0.0m;
    }

    private decimal GetMockFiftyTwoWeekLow(string symbol)
    {
      var lows = new Dictionary<string, decimal>
    {
        { "AAPL", 120.00m },
        { "MSFT", 220.00m },
        { "GOOGL", 2500.00m }
        // Add more mappings as needed
    };

      return lows.ContainsKey(symbol) ? lows[symbol] : 0.0m;
    }

    private long GetMockVolume(string symbol)
    {
      var volumes = new Dictionary<string, long>
    {
        { "AAPL", 75000000 },
        { "MSFT", 60000000 },
        { "GOOGL", 50000000 }
        // Add more mappings as needed
    };

      return volumes.ContainsKey(symbol) ? volumes[symbol] : 0;
    }

    private double GetMockDividendYield(string symbol)
    {
      var dividendYields = new Dictionary<string, double>
    {
        { "AAPL", 0.6 },
        { "MSFT", 0.8 },
        { "GOOGL", 0.0 } // GOOG doesn't pay dividends
        // Add more mappings as needed
    };

      return dividendYields.ContainsKey(symbol) ? dividendYields[symbol] : 0.0;
    }
    private string GetMockSector(string symbol)
    {
      var sectors = new Dictionary<string, string>
            {
                { "AAPL", "Technology" },
                { "MSFT", "Technology" },
                { "GOOGL", "Communication Services" }
                // Add more mappings as needed
            };

      return sectors.ContainsKey(symbol) ? sectors[symbol] : "N/A";
    }

    private string GetMockIndustry(string symbol)
    {
      var industries = new Dictionary<string, string>
            {
                { "AAPL", "Consumer Electronics" },
                { "MSFT", "Software—Infrastructure" },
                { "GOOGL", "Internet Content & Information" }
                // Add more mappings as needed
            };

      return industries.ContainsKey(symbol) ? industries[symbol] : "N/A";
    }

    private string GetMockDescription(string symbol)
    {
      var descriptions = new Dictionary<string, string>
            {
                { "AAPL", "Apple Inc. designs, manufactures, and markets smartphones, personal computers, tablets, wearables, and accessories worldwide." },
                { "MSFT", "Microsoft Corporation develops, licenses, and supports a range of software products, services, and devices worldwide." },
                { "GOOGL", "Alphabet Inc. provides online advertising services in the United States, Europe, the Middle East, Africa, the Asia-Pacific, Canada, and Latin America." }
                // Add more mappings as needed
            };

      return descriptions.ContainsKey(symbol) ? descriptions[symbol] : "No description available.";
    }

    private string GetMockCEO(string symbol)
    {
      var ceos = new Dictionary<string, string>
            {
                { "AAPL", "Tim Cook" },
                { "MSFT", "Satya Nadella" },
                { "GOOGL", "Sundar Pichai" }
                // Add more mappings as needed
            };

      return ceos.ContainsKey(symbol) ? ceos[symbol] : "N/A";
    }

    private List<ChartDataPoint> GetMockChartData(string symbol)
    {
      // Generate mock chart data for the past 30 days
      var chartData = new List<ChartDataPoint>();
      var random = new Random();
      var today = DateTime.Today;

      for (int i = 30; i >= 1; i--)
      {
        chartData.Add(new ChartDataPoint
        {
          Date = today.AddDays(-i),
          Close = (decimal)(100 + random.NextDouble() * 100) // Random close price between 100 and 200
        });
      }

      return chartData;
    }

    private List<NewsItemScreener> GetMockNews(string symbol)
    {
      // Mock news articles
      return new List<NewsItemScreener>
            {
                new NewsItemScreener
                {
                    Title = $"{symbol} releases new product",
                    Url = "https://example.com/news1",
                    PublishedDate = DateTime.Today.AddDays(-1),
                    Source = "Example News"
                },
                new NewsItemScreener
                {
                    Title=($"{symbol} stock hits new high"),
                    Url = "https://example.com/news2",
                    PublishedDate = DateTime.Today.AddDays(-2),
                    Source = "Finance Today"
                },
                // Add more mock news as needed
            };
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

