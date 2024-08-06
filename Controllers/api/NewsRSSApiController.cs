using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using HtmlAgilityPack;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class NewsRSSApiController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsRSSApiController> _logger;

    public NewsRSSApiController(HttpClient httpClient, ILogger<NewsRSSApiController> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
    }

    [HttpGet("GetRssUrls")]
    public IActionResult GetRssUrls()
    {
      var urls = new Dictionary<string, string>
            {
                { "Technology", "https://rss.nytimes.com/services/xml/rss/nyt/Technology.xml" },
                { "Business", "https://rss.nytimes.com/services/xml/rss/nyt/Business.xml" },
                { "YourMoney", "https://rss.nytimes.com/services/xml/rss/nyt/YourMoney.xml" },
                { "SmallBusiness", "https://rss.nytimes.com/services/xml/rss/nyt/SmallBusiness.xml" },
                { "Economy", "https://rss.nytimes.com/services/xml/rss/nyt/Economy.xml" },
                { "TechCrunch", "https://techcrunch.com/feed/" },
                { "Wired", "https://www.wired.com/feed/rss" },
                { "The Verge", "https://www.theverge.com/rss/index.xml" },
                { "Forbes", "https://www.forbes.com/business/feed/" },
                { "Reuters", "http://feeds.reuters.com/reuters/businessNews" },
                { "Economist", "http://www.economist.com/sections/economics/rss.xml" },
                { "FT", "http://www.ft.com/rss/home/us" },
                { "MarketWatch", "http://feeds.marketwatch.com/marketwatch/topstories/" },
                { "Entrepreneur", "https://www.entrepreneur.com/latest/rss" },
                { "Inc", "https://www.inc.com/rss" },
                { "Mashable", "http://feeds.mashable.com/Mashable" },
                { "CNET", "https://www.cnet.com/rss/news/" },
                { "Ars Technica", "http://feeds.arstechnica.com/arstechnica/index/" },
                { "CNBC", "https://www.cnbc.com/id/10001147/device/rss/rss.html" },
                { "Fortune", "http://fortune.com/feed/" },
                { "Business Insider", "https://www.businessinsider.com/sai/rss" },
                { "IMF", "https://www.imf.org/external/np/exr/rss/whatnew.xml" },
                { "OECD", "https://www.oecd.org/newsroom/news-releases-rss.xml" },
                { "Small Business Trends", "https://smallbiztrends.com/feed" },
                { "AllBusiness", "https://www.allbusiness.com/feed" },
                { "BBC News", "http://feeds.bbci.co.uk/news/rss.xml" },
                { "CNN", "http://rss.cnn.com/rss/edition.rss" },
                { "Al Jazeera", "https://www.aljazeera.com/xml/rss/all.xml" }
            };

      return Ok(urls);
    }

    [HttpGet("FetchRssFeed")]
    public async Task<IActionResult> FetchRssFeed(string category, string url)
    {
      var newsItems = new List<RssNewsItem>();

      try
      {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        using (var stringReader = new System.IO.StringReader(responseBody))
        {
          var settings = new XmlReaderSettings
          {
            DtdProcessing = DtdProcessing.Ignore
          };

          using (var xmlReader = XmlReader.Create(stringReader, settings))
          {
            var feed = SyndicationFeed.Load(xmlReader);
            var imageTasks = new List<Task>();

            foreach (var item in feed.Items)
            {
              var newsItem = new RssNewsItem
              {
                Title = item.Title?.Text,
                Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                Description = item.Summary?.Text,
                Author = item.Authors.FirstOrDefault()?.Name,
                Date = item.PublishDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Category = category
              };

              newsItems.Add(newsItem);
              // imageTasks.Add(SetImageUrlAsync(newsItem));
            }

            await Task.WhenAll(imageTasks);
          }
        }
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, $"Error fetching RSS feed for category {category} from {url}.");
        return StatusCode(500, $"Error fetching RSS feed for category {category} from {url}.");
      }
      catch (XmlException xmlEx)
      {
        _logger.LogError(xmlEx, $"Error parsing XML for category {category} from {url}.");
        return StatusCode(500, $"Error parsing XML for category {category} from {url}.");
      }

      return Ok(newsItems);
    }

    [HttpPost("EditNews")]
    public IActionResult EditNews(string id)
    {
      // Implement edit functionality
      return NoContent();
    }

    [HttpPost("DeleteNews")]
    public IActionResult DeleteNews(string id)
    {
      // Implement delete functionality
      return NoContent();
    }

    [HttpPost("RunJob")]
    public IActionResult RunJob(string id)
    {
      // Implement run job functionality
      return NoContent();
    }
  }

  public class RssNewsItem
  {
    public string Title { get; set; }
    public string Link { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Date { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
  }
}
