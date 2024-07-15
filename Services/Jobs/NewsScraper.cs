using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MarketAnalyticHub.Services.Jobs
{
  public class NewsScraper
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NewsScraper> _logger;
    private readonly IConfiguration _configuration;

    public NewsScraper(IHttpClientFactory httpClientFactory, ILogger<NewsScraper> logger, IConfiguration configuration)
    {
      _httpClientFactory = httpClientFactory;
      _logger = logger;
      _configuration = configuration;
    }

    public async Task ScrapeNewsAsync()
    {
      try
      {
        var httpClient = _httpClientFactory.CreateClient();

        // Example endpoints to scrape news
        var endpoint1 = _configuration["NewsScrapingEndpoints:Endpoint1"];
        var endpoint2 = _configuration["NewsScrapingEndpoints:Endpoint2"];

        // Scraping from endpoint 1
        var response1 = await httpClient.GetAsync(endpoint1);
        if (response1.IsSuccessStatusCode)
        {
          var content1 = await response1.Content.ReadAsStringAsync();
          _logger.LogInformation("News scraped successfully from {Endpoint} at {Time}", endpoint1, DateTime.UtcNow);
        }
        else
        {
          _logger.LogError("Failed to scrape news from {Endpoint} at {Time}", endpoint1, DateTime.UtcNow);
        }

        // Scraping from endpoint 2
        var response2 = await httpClient.GetAsync(endpoint2);
        if (response2.IsSuccessStatusCode)
        {
          var content2 = await response2.Content.ReadAsStringAsync();
          _logger.LogInformation("News scraped successfully from {Endpoint} at {Time}", endpoint2, DateTime.UtcNow);
        }
        else
        {
          _logger.LogError("Failed to scrape news from {Endpoint} at {Time}", endpoint2, DateTime.UtcNow);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Exception occurred while scraping news");
      }
    }
  }
}
