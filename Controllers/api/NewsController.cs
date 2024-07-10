using AspnetCoreMvcFull.Models.News;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class NewsController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsController> _logger;
    public NewsController(HttpClient httpClient, ILogger<NewsController> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
    }
    [HttpGet("scraped")]
    public async Task<IEnumerable<NewsItem>> GetScrapedNewsAsync()
    {
      var url = "https://news.google.com/search?q=world&hl=en-US&gl=US&ceid=US%3Aen";
      HttpResponseMessage response;
      try
      {
        response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Error fetching news from Google News.");
        throw;
      }

      var responseBody = await response.Content.ReadAsStringAsync();
      _logger.LogInformation("Fetched news page: {PageContent}", responseBody);

      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(responseBody);

      var newsNodes = htmlDoc.DocumentNode.SelectNodes("//h3");
      if (newsNodes == null)
      {
        _logger.LogWarning("No news items found on the fetched page.");
        return Enumerable.Empty<NewsItem>();
      }

      var newsItems = newsNodes
          .Select(node => new NewsItem
          {
            Title = node.InnerText,
            Link = "https://news.google.com" + node.SelectSingleNode(".//a")?.GetAttributeValue("href", string.Empty).Substring(1) // To form the correct link
          })
          .ToList();

      return newsItems;
    }

    [HttpGet("hardcoded")]
    public ActionResult<IEnumerable<NewsItem>> GetHardcodedNews()
    {
      var news = new List<NewsItem>
            {
                new NewsItem { Category = "Technology", Title = "New Angular Version Released", Description = "Angular has released its latest version with significant improvements...", Date = "10 July 2024" },
                new NewsItem { Category = "Science", Title = "NASA's New Discovery", Description = "NASA has discovered a new exoplanet that may have water...", Date = "09 July 2024" },
                new NewsItem { Category = "Health", Title = "Breakthrough in Cancer Research", Description = "Scientists have made a significant breakthrough in cancer treatment...", Date = "08 July 2024" },
                new NewsItem { Category = "Finance", Title = "Stock Market Hits New High", Description = "The stock market has reached a new all-time high, driven by tech stocks...", Date = "07 July 2024" }
            };

      return Ok(news);
    }
  }
}
