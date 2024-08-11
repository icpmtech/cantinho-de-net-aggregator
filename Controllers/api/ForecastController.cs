using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Services.Elastic;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MarketAnalyticHub.Models.SetupDb;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ForecastController : ControllerBase
  {
    private readonly ElasticSearchService _elasticSearchService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ForecastController> _logger;

    public ForecastController(ApplicationDbContext context, ElasticSearchService elasticSearchService, ILogger<ForecastController> logger)
    {
      _elasticSearchService = elasticSearchService;
      _context = context;
      _logger = logger;
    }

    /// <summary>
    /// Retrieves historical investment data.
    /// </summary>
    [HttpGet("historical-data")]
    public async Task<IActionResult> GetHistoricalData()
    {
      try
      {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var indexName = $"user-id-{userId}-portfolio";

        var searchResponse = await _elasticSearchService._client.SearchAsync<PortfolioItem>(s => s
            .Index(indexName)
            .Query(q => q
                .DateRange(dr => dr
                    .Field(f => f.PurchaseDate)
                    .GreaterThanOrEquals("2024-01-01T00:00:00")
                    .LessThanOrEquals("2024-12-31T23:59:59")
                )
            )
            .Sort(st => st
                .Descending(f => f.PurchaseDate)
            )
        );

        if (!searchResponse.IsValid)
        {
          _logger.LogError("Failed to retrieve historical data: {ErrorMessage}", searchResponse.OriginalException.Message);
          return StatusCode(500, "Error retrieving historical data.");
        }

        var historicalData = searchResponse.Documents.Select(d => new
        {
          x = d.PurchaseDate.ToString("yyyy-MM-dd"),
          y = d.TotalInvestment
        }).ToList();

        return Ok(historicalData);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while retrieving historical data.");
        return StatusCode(500, "An error occurred while retrieving historical data.");
      }
    }

    /// <summary>
    /// Adds new forecast data.
    /// </summary>
    [HttpPost("add-forecast")]
    public async Task<IActionResult> AddForecast([FromBody] ForecastData forecastData)
    {
      try
      {
        var indexName = $"user-id-{User.FindFirstValue(ClaimTypes.NameIdentifier)}-forecast";

        var indexResponse = await _elasticSearchService._client.IndexAsync(new ForecastItem
        {
          Date = forecastData.Date,
          Investment = forecastData.Investment
        }, idx => idx.Index(indexName));

        if (!indexResponse.IsValid)
        {
          _logger.LogError("Failed to add forecast data: {ErrorMessage}", indexResponse.OriginalException.Message);
          return StatusCode(500, "Error adding forecast data.");
        }

        return Ok();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while adding forecast data.");
        return StatusCode(500, "An error occurred while adding forecast data.");
      }
    }

    /// <summary>
    /// Retrieves forecast data.
    /// </summary>
    [HttpGet("forecast-data")]
    public async Task<IActionResult> GetForecastData()
    {
      try
      {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var indexName = $"user-id-{userId}-forecast";

        var searchResponse = await _elasticSearchService._client.SearchAsync<ForecastItem>(s => s
            .Index(indexName)
            .Sort(st => st
                .Ascending(f => f.Date)
            )
        );

        if (!searchResponse.IsValid)
        {
          _logger.LogError("Failed to retrieve forecast data: {ErrorMessage}", searchResponse.OriginalException.Message);
          return StatusCode(500, "Error retrieving forecast data.");
        }

        var forecastData = searchResponse.Documents.Select(d => new
        {
          x = d.Date.ToString("yyyy-MM-dd"),
          y = d.Investment
        }).ToList();

        return Ok(forecastData);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while retrieving forecast data.");
        return StatusCode(500, "An error occurred while retrieving forecast data.");
      }
    }
  }

  public class ForecastItem
  {
    public DateTime Date { get; set; }
    public double Investment { get; set; }
  }

  public class ForecastData
  {
    public DateTime Date { get; set; }
    public double Investment { get; set; }
  }
}
