

namespace MarketAnalyticHub.Controllers.api
{
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Options;
  using System.Net.Http;
  using System.Threading.Tasks;
  using System.Collections.Generic;
  using System.Linq;
  using Newtonsoft.Json.Linq;
  using Newtonsoft.Json;
  using NewsAPI.Constants;
  using NewsAPI.Models;
  using NewsAPI;

  [ApiController]
  [Route("api/[controller]")]
  public class NewsApiController : ControllerBase
  {
    private readonly NewsApiSettings _newsApiSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsApiController> _logger;
    public NewsApiController(IOptions<NewsApiSettings> newsApiSettings, ILogger<NewsApiController> logger)
    {
      _newsApiSettings = newsApiSettings.Value;
      _httpClient = new HttpClient();
      _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] string startDate, [FromQuery] string endDate)
    {
      if (string.IsNullOrEmpty(query))
      {
        _logger.LogWarning("Query parameter is required.");
        return BadRequest("Query parameter is required.");
      }

      try
      {
        _logger.LogInformation("Sending request to News API with query: {Query}", query);
        var newsApiClient = new NewsApiClient(_newsApiSettings.ApiKey);
        var articlesResponse = await Task.Run(() => newsApiClient.GetEverything(new EverythingRequest
        {
          Q = query,
          SortBy = SortBys.Popularity,
          Language = Languages.EN,
          From = string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate),
          To = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate)
        }));

        if (articlesResponse.Status == Statuses.Ok)
        {
          _logger.LogInformation("Received response from News API with {TotalResults} results.", articlesResponse.TotalResults);
          var articles = articlesResponse.Articles;
          return Ok(articles);
        }
        else
        {
          _logger.LogWarning("News API response status: {Status}", articlesResponse.Status);
          return StatusCode(500, "Error occurred while fetching news from News API.");
        }
      }
      catch (HttpRequestException httpEx)
      {
        _logger.LogError(httpEx, "Error occurred while sending HTTP request.");
        return StatusCode(503, "Error occurred while fetching news. Please try again later.");
      }
      catch (JsonException jsonEx)
      {
        _logger.LogError(jsonEx, "Error occurred while parsing JSON response.");
        return StatusCode(500, "Error occurred while processing news data. Please try again later.");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An unexpected error occurred.");
        return StatusCode(500, "An unexpected error occurred. Please try again later.");
      }
    }
  }
}
