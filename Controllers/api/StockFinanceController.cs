

using MarketAnalyticHub.Controllers.Configurations.Reddit;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static MarketAnalyticHub.Services.PortfolioService;

namespace MarketAnalyticHub.Controllers.api
{

  [ApiController]
  [Route("api/stockfinance")]
  public class StockFinanceController : ControllerBase
  {
    private readonly PortfolioService _portfolioService;
    private readonly NewsService _newsService;
    private readonly ILogger<PortfolioScrenerController> _logger;
    private readonly ApplicationDbContext _context;
     DividendService _dividendService;
    private readonly IYahooFinanceService _yahooFinanceService;

    public StockFinanceController(IYahooFinanceService yahooFinanceService, DividendService dividendService, ApplicationDbContext context, PortfolioService portfolioService, NewsService newsService, ILogger<PortfolioScrenerController> logger)
    {
      _newsService = newsService;
      _portfolioService = portfolioService;
      _logger = logger;
      _context = context;
      _yahooFinanceService = yahooFinanceService;
      _dividendService = dividendService;
    }
    /// <summary>
    /// Obtém dados históricos de uma ação em um intervalo de datas específico.
    /// </summary>
    /// <param name="symbol">Símbolo da ação (ex: AAPL).</param>
    /// <param name="startDate">Data de início.</param>
    /// <param name="endDate">Data de fim.</param>
    /// <param name="interval">Intervalo de dados (ex: 1d, 1wk, 1mo). Padrão: 1d.</param>
    /// <returns>Dados históricos da ação.</returns>
    [HttpGet("GetHistoricalData")]
    public async Task<IActionResult> GetHistoricalData(
        [FromQuery] string symbol,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval = "1d",
        CancellationToken token = default)
    {
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest(new { Message = "Symbol is required." });
      }

      if (endDate < startDate)
      {
        return BadRequest(new { Message = "End date must be greater than or equal to start date." });
      }

      try
      {
        var historicalData = await YahooService.GetHistoricalJsonDataAsync(symbol, startDate , endDate, token);

        if (historicalData == null )
        {
          return Ok(new { Message = "No historical data found.", Data = (object)null });
        }

       

        return Ok(historicalData);
      }
      catch (Exception ex)
      {
        // Log the exception details as needed
        Console.WriteLine($"Error in GetHistoricalData: {ex.Message}");
        return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
      }
    }
    [HttpGet("search-finance")]
    public async Task<ActionResult> Details(string symbol)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      // Validate input
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest("The stock symbol must be provided.");
      }

      // Initialize the ViewModel
      var viewModel = new DetailScreenerViewModel
      {
        HasQuery = !string.IsNullOrWhiteSpace(symbol),
        Stock = new StockViewModel()
      };

      try
      {
        if (viewModel.HasQuery)
        {
          // Fetch stock data
          PortfolioCardDto? portfolioCardDto =null;
          try
          {
            portfolioCardDto  = await _portfolioService.GetPortfolioBySymbolAsync(userId,  symbol);
          }
          catch (Exception ex )
          {
            _logger.LogError(ex, "An error occurred while fetching GetPortfolioBySymbolAsync details for symbol: {Symbol}", symbol);

          }
        
          var stock = await _yahooFinanceService.GetStockDataAsync(symbol);
          var summary = await _yahooFinanceService.GetSummaryBySymbolAsync(symbol);

          if (stock != null && summary != null)
          {
            // Map summary data to the stock model
            stock.Industry = summary.Industry;
            stock.CEO = summary.CEO;
            stock.Sector = summary.Sector;
            stock.Description = summary.Description;
            stock.DataCardForSymbol = portfolioCardDto;
            // Fetch historical chart data
            stock.ChartData = await _yahooFinanceService.GetHistoricalDataAsync(
                symbol,
                DateTime.Today.AddMonths(-1),
                DateTime.Today
            );

            // Fetch additional data
            stock.TechnicalSignals = GetTechnicalSignals(symbol); // Method for calculating technical signals
            stock.AnalystRatings = GetAnalystRatings(symbol); // Method for fetching analyst ratings
            stock.DividendYield = summary.DividendYield; // Assuming this is part of the summary
            stock.Dividends = await _dividendService.GetDividendsAsync(symbol, DateTime.Now.AddYears(-15), DateTime.Now);
            stock.News = await _yahooFinanceService.GetMockNews(symbol); // Replace with actual implementation
            stock.SentimentScore = GetMockSentiment(symbol); // Replace with actual sentiment calculation

            // Populate the ViewModel
            viewModel.Stock = stock;
          }
        }

        // Return the ViewModel
        return Ok(viewModel);
      }
      catch (Exception ex)
      {
        // Log the error for debugging
        _logger.LogError(ex, "An error occurred while fetching stock details for symbol: {Symbol}", symbol);

        // Return a server error response
        return StatusCode(500, "An error occurred while processing your request. Please try again later.");
      }
    }


    [HttpGet("search-finance-data-symbol/{symbol}")]
    public async Task<ActionResult> DataSymbol(string symbol)
    {
     
      // Validate input
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest("The stock symbol must be provided.");
      }

      // Initialize the ViewModel
      var viewModel = new DetailScreenerViewModel
      {
        HasQuery = !string.IsNullOrWhiteSpace(symbol),
        Stock = new StockViewModel()
      };

      try
      {
          var stock = await _yahooFinanceService.GetStockDataAsync(symbol);
          var summary = await _yahooFinanceService.GetSummaryBySymbolAsync(symbol);

          if (stock != null && summary != null)
          {
            // Map summary data to the stock model
            stock.Industry = summary.Industry;
            stock.CEO = summary.CEO;
            stock.Sector = summary.Sector;
            stock.Description = summary.Description;
            // Fetch historical chart data
            stock.ChartData = await _yahooFinanceService.GetHistoricalDataAsync(
                symbol,
                DateTime.Today.AddMonths(-1),
                DateTime.Today
            );

            // Fetch additional data
            stock.TechnicalSignals = GetTechnicalSignals(symbol); // Method for calculating technical signals
            stock.AnalystRatings = GetAnalystRatings(symbol); // Method for fetching analyst ratings
            stock.DividendYield = summary.DividendYield; // Assuming this is part of the summary
          stock.Dividends = await _dividendService.GetDividendsAsync(symbol, DateTime.Now.AddYears(-15), DateTime.Now);
          stock.News = await _yahooFinanceService.GetMockNews(symbol); // Replace with actual implementation
            stock.SentimentScore = GetMockSentiment(symbol); // Replace with actual sentiment calculation

            // Populate the ViewModel
            viewModel.Stock = stock;
          }

        // Return the ViewModel
        return Ok(viewModel);
      }
      catch (Exception ex)
      {
        // Log the error for debugging
        _logger.LogError(ex, "An error occurred while fetching stock details for symbol: {Symbol}", symbol);

        // Return a server error response
        return StatusCode(500, "An error occurred while processing your request. Please try again later.");
      }
    }

    [HttpGet("search-finance-finantials-symbol/{symbol}")]
    public async Task<ActionResult> DataFinancialsSymbol(string symbol)
    {

      // Validate input
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest("The stock symbol must be provided.");
      }

      // Initialize the ViewModel
      var viewModel = new DetailStockFinancialsViewModel
      {
        HasQuery = !string.IsNullOrWhiteSpace(symbol),
        Stock = new StockFinantialsViewModel(),
        StockBase = new StockViewModel()
      };

      try
      {
        var stockData = await _yahooFinanceService.GetStockDataAsync(symbol);
        var stock = await _yahooFinanceService.GetFinancialsBySymbolAsync(symbol);

        if (stock != null && stockData!=null)
        {
          viewModel.Stock = stock;
          viewModel.StockBase = stockData;
        }

        // Return the ViewModel
        return Ok(viewModel);
      }
      catch (Exception ex)
      {
        // Log the error for debugging
        _logger.LogError(ex, "An error occurred while fetching stock details for symbol: {Symbol}", symbol);

        // Return a server error response
        return StatusCode(500, "An error occurred while processing your request. Please try again later.");
      }
    }


    private List<DividendScreenViewModel> GetDividends(string symbol)
    {
      // Replace with actual logic or API calls to fetch dividends
      return new List<DividendScreenViewModel>
    {
        new DividendScreenViewModel { Date = "2024-11-15", ExDate = "2024-11-10", Amount = "0.10 EUR" },
        new DividendScreenViewModel { Date = "2024-08-15", ExDate = "2024-08-10", Amount = "0.12 EUR" }
    };
    }

    private Dictionary<string, int> GetAnalystRatings(string symbol)
    {
      // Replace with actual logic or API calls to get analyst ratings
      return new Dictionary<string, int>
    {
        { "Buy", 60 },
        { "Hold", 30 },
        { "Sell", 10 }
    };
    }

    private Dictionary<string, decimal> GetTechnicalSignals(string symbol)
    {
      // Replace with actual logic or API calls to calculate technical signals
      return new Dictionary<string, decimal>
    {
        { "RSI", 70 },
        { "MACD", 80 },
        { "MovingAverage", 60 }
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
