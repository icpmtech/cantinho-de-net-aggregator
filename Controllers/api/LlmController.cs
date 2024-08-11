using AspnetCoreMvcFull.Services;
using DocumentFormat.OpenXml.InkML;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Services.Elastic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Nest;
using System.Globalization;
using System.Security.Claims;
using static MarketAnalyticHub.Controllers.SocialSentimentService;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Authorize]
  [Route("api/[controller]")]
  public class LlmController : ControllerBase
  {
    private readonly LlmService _llmService;
    private readonly DataIndexingService _dataIndexingService;
    private readonly ElasticSearchService _elasticSearchService;
    private readonly ApplicationDbContext _context;


    public LlmController(ApplicationDbContext context, ElasticSearchService elasticSearchService, DataIndexingService dataIndexingService, LlmService llmService)
    {
      _llmService = llmService;
      _dataIndexingService = dataIndexingService;
      _elasticSearchService = elasticSearchService;
      _context = context;

    }

    [HttpGet("search-company-summary/{symbol}")]
    public async Task<IActionResult> GetCompanySummaryAsync(string symbol)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      // Fetch summary from LLM Open AI Service
      var summary = await _llmService.GetSymbolSummaryAsync(symbol);


      return Ok(summary);
    }
    [HttpGet("area-forecast-data")]
    public async Task<IActionResult> GetAreaForecastData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      // Fetch historical data
      var items = await _dataIndexingService.GetPortfolioItemsAsync(userId);

      // Example data
      var historicalData = items.Select(item => new
      {
        x = item.PurchaseDate,
        y = item.TotalInvestment
      }).ToList();

      var lowerBound = new List<object>
    {
        new { x = "2024-09-01", y = 220.0 },
        new { x = "2024-10-01", y = 270.0 },
        new { x = "2024-11-01", y = 320.0 }
    };

      var upperBound = new List<object>
    {
        new { x = "2024-09-01", y = 280.0 },
        new { x = "2024-10-01", y = 330.0 },
        new { x = "2024-11-01", y = 380.0 }
    };

      var data = new
      {
        historical = historicalData,
        lowerBound,
        upperBound
      };

      return Ok(data);
    }

    [HttpGet("pl-month-data")]
    public async Task<IActionResult> GetPlMonthData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var items = await _dataIndexingService.GetPortfolioItemsAsync(userId);

      // Group by month and calculate P/L
      var data = items
          .GroupBy(item => new { item.PurchaseDate.Year, item.PurchaseDate.Month })
          .Select(group => new
          {
            x = new DateTime(group.Key.Year, group.Key.Month, 1),
            y = group.Sum(item => item.TotalInvestment) // Replace with P/L calculation
          })
          .ToList();

      return Ok(data);
    }

    [HttpGet("pl-week-data")]
    public async Task<IActionResult> GetPlWeekData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var items = await _dataIndexingService.GetPortfolioItemsAsync(userId);

      // Group by week and calculate P/L
      var data = items
          .GroupBy(item => new { Year = item.PurchaseDate.Year, Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(item.PurchaseDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) })
          .Select(group => new
          {
            x = group.First().PurchaseDate.AddDays(-((int)group.First().PurchaseDate.DayOfWeek - 1)), // Start of the week
            y = group.Sum(item => item.TotalInvestment) // Replace with P/L calculation
          })
          .ToList();

      return Ok(data);
    }

    [HttpGet("bubble-trend-data")]
    public async Task<IActionResult> GetBubbleTrendData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      // Fetch historical data
      var items = await _dataIndexingService.GetPortfolioItemsAsync(userId);

      // Example data
      var data = items.Select(item => new
      {
        x = item.PurchaseDate,
        y = item.TotalInvestment,
        z = item.Quantity // Bubble size
      }).ToList();

      // Example trend line data
      var trendLine = new List<object>
    {
        new { x = "2024-08-01", y = 200.0 },
        new { x = "2024-09-01", y = 240.0 },
        new { x = "2024-10-01", y = 280.0 }
    };

      var result = new
      {
        bubbles = data,
        trendLine
      };

      return Ok(result);
    }
    [HttpGet("portfolio-summary")]
    public async Task<IActionResult> GetPortfolioSummary()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var items = await _dataIndexingService.GetPortfolioItemsAsync(userId);

      var data = items
          .GroupBy(item => item.Symbol)
          .Select(group => new
          {
            name = group.Key,
            y = group.Sum(item => item.TotalInvestment)
          })
          .ToList();

      return Ok(data);
    }



    [HttpGet("portfolio-data")]
    public async Task<IActionResult> GetPortfolioData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var items = await _dataIndexingService.GetPortfolioItemsAsync(userId);

      var data = items.Select(item => new
      {
        x = item.PurchaseDate, // Use DateTime for x-axis
        y = item.TotalInvestment, // Or other metrics
        symbol = item.Symbol,
        sentimentScore = item.SocialSentiment.SentimentScore
      }).ToList();

      return Ok(data);
    }


    [HttpGet("portfolio-value/{portfolioId}")]
    public async Task<IActionResult> GetPortfolioValue(int portfolioId)
    {
      var metrics = await _dataIndexingService.GetPortfolioValueAsync(portfolioId);
      return Ok(metrics.Aggregations);
    }
    [HttpPost("handle-natural-language-query")]
    public async Task<IActionResult> HandleNaturalLanguageQuery(string query)
    {
      var prompt = $"Analyze the portfolio based on the following query: {query}";
      var analysis = await _llmService.GeneratePortfolioReportAsync(prompt);
      return Ok(analysis);
    }


    [HttpPost("analyze-sentiment-for-stock")]
    public async Task<IActionResult> AnalyzeSentimentForStock(string tickerSymbol)
    {
      var prompt = $"Analyze the market sentiment for the stock {tickerSymbol}.";
      var sentiment = await _llmService.GeneratePortfolioReportAsync(prompt);
      return Ok(sentiment);
    }


    [HttpPost("investment-suggestions")]
    public async Task<IActionResult> GetInvestmentSuggestions(string portfolioSummary)
    {
      var prompt = $"Based on the following portfolio summary, suggest potential investment opportunities: {portfolioSummary}";
      var suggestions = await _llmService.GeneratePortfolioReportAsync(prompt);
      return Ok(suggestions);
    }

    [HttpPost("generate-report")]
    public async Task<IActionResult> GenerateReport([FromBody] string prompt)
    {
      var report = await _llmService.GeneratePortfolioReportAsync(prompt);
      return Ok(report);
    }
  }

}
