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
      var url = "https://www.nytimes.com/section/business/economy";
      HttpResponseMessage response;
      try
      {
        response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Error fetching news from New York Times.");
        throw;
      }

      var responseBody = await response.Content.ReadAsStringAsync();
      _logger.LogInformation("Fetched news page.");

      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(responseBody);

      var newsNodes = htmlDoc.DocumentNode.SelectNodes("//li[@class='css-18yolpw']");

      if (newsNodes == null)
      {
        _logger.LogWarning("No news items found on the fetched page.");
        return Enumerable.Empty<NewsItem>();
      }

      var newsItems = new List<NewsItem>();

      foreach (var node in newsNodes)
      {
        var titleNode = node.SelectSingleNode(".//h3[@class='css-1j88qqx e15t083i0']");
        var linkNode = node.SelectSingleNode(".//a[@class='css-8hzhxf']");
        var descriptionNode = node.SelectSingleNode(".//p[@class='css-1pga48a e15t083i1']");
        var authorNode = node.SelectSingleNode(".//div[@class='css-1i4y2t3 e140qd2t0']//span[@class='css-1n7hynb']");
        var dateNode = node.SelectSingleNode(".//div[@class='css-e0xall e15t083i3']//span[@data-testid='todays-date']");

        if (titleNode == null || linkNode == null)
        {
          continue;
        }

        var title = titleNode.InnerText.Trim();
        var link = linkNode.GetAttributeValue("href", string.Empty);
        var description = descriptionNode?.InnerText.Trim();
        var author = authorNode?.InnerText.Trim();
        var date = dateNode?.InnerText.Trim();

        // Ensure the link is well-formed
        if (!link.StartsWith("http"))
        {
          link = "https://www.nytimes.com" + link;
        }

        newsItems.Add(new NewsItem
        {
          Title = title,
          Link = link,
          Description = description,
          Author = author,
          Date = date,
          Category = "Economy" // Fixed category for this example
        });
      }

      return newsItems;
    }
    [HttpGet("scraped-env")]
    public async Task<IEnumerable<NewsItem>> GetScrapedNewsEnvAsync()
    {
      var url = "https://www.nytimes.com/section/business/energy-environment";
      HttpResponseMessage response;
      try
      {
        response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Error fetching news from New York Times.");
        throw;
      }

      var responseBody = await response.Content.ReadAsStringAsync();
      _logger.LogInformation("Fetched news page.");

      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(responseBody);

      var newsNodes = htmlDoc.DocumentNode.SelectNodes("//li[contains(@class, 'css-18yolpw')]");

      if (newsNodes == null)
      {
        _logger.LogWarning("No news items found on the fetched page.");
        return Enumerable.Empty<NewsItem>();
      }

      var newsItems = new List<NewsItem>();

      foreach (var node in newsNodes)
        {
          var titleNode = node.SelectSingleNode(".//h3[contains(@class, 'css-1j88qqx')]");
          var linkNode = node.SelectSingleNode(".//a[contains(@class, 'css-8hzhxf')]");
          var descriptionNode = node.SelectSingleNode(".//p[contains(@class, 'css-1pga48a')]");
          var authorNode = node.SelectSingleNode(".//div[contains(@class, 'css-1i4y2t3')]//span[contains(@class, 'css-1n7hynb')]");
          var dateNode = node.SelectSingleNode(".//div[contains(@class, 'css-e0xall')]//span[@data-testid='todays-date']");

          if (titleNode == null || linkNode == null)
          {
            continue;
          }

          var title = titleNode.InnerText.Trim();
          var link = linkNode.GetAttributeValue("href", string.Empty);
          var description = descriptionNode?.InnerText.Trim();
          var author = authorNode?.InnerText.Trim();
          var date = dateNode?.InnerText.Trim();

          // Ensure the link is well-formed
          if (!link.StartsWith("http"))
          {
            link = "https://www.nytimes.com" + link;
          }

          newsItems.Add(new NewsItem
          {
            Title = title,
            Link = link,
            Description = description,
            Author = author,
            Date = date,
            Category = "Energy and Environment" // Assign the category based on the URL
          });
        }

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
