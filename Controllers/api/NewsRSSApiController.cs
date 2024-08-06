using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class NewsRSSApiController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsRSSApiController> _logger;
    private readonly ApplicationDbContext _context;

    public NewsRSSApiController(HttpClient httpClient, ApplicationDbContext context, ILogger<NewsRSSApiController> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
      _context = context;
    }

    [HttpGet("GetRssUrls")]
    public async Task<IActionResult> GetRssUrls()
    {
      try
      {
        var rssLinks = await _context.RSSLinks.ToListAsync();
        var urls = rssLinks.ToDictionary(link => link.Category, link => link.Url);
        return Ok(urls);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving RSS URLs.");
        return StatusCode(500, "Error retrieving RSS URLs.");
      }
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
            foreach (var item in feed.Items)
            {
              var newsItem = new RssNewsItem
              {
                Title = item.Title?.Text,
                Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                Description = item.Summary?.Text,
                Author = item.Authors.FirstOrDefault()?.Name,
                Date = item.PublishDate.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Category = category
              };

              newsItems.Add(newsItem);
            }
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
    public async Task<IActionResult> EditNews([FromBody] RssNewsItem updatedNewsItem)
    {
      try
      {
        var newsItem = await _context.News.FindAsync(updatedNewsItem.Id);
        if (newsItem == null)
        {
          return NotFound("News item not found.");
        }

        newsItem.Title = updatedNewsItem.Title;
        newsItem.Link = updatedNewsItem.Link;
        newsItem.Description = updatedNewsItem.Description;
        newsItem.Author = updatedNewsItem.Author;
        newsItem.Date = updatedNewsItem.Date;
        newsItem.Category = updatedNewsItem.Category;

        await _context.SaveChangesAsync();

        return Ok(newsItem);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error editing news item.");
        return StatusCode(500, "Error editing news item.");
      }
    }

    [HttpPost("DeleteNews")]
    public async Task<IActionResult> DeleteNews([FromBody] string id)
    {
      try
      {
        var newsItem = await _context.News.FindAsync(id);
        if (newsItem == null)
        {
          return NotFound("News item not found.");
        }

        _context.News.Remove(newsItem);
        await _context.SaveChangesAsync();

        return NoContent();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting news item.");
        return StatusCode(500, "Error deleting news item.");
      }
    }

    [HttpPost("RunJob")]
    public IActionResult RunJob(string id)
    {
      // Implement run job functionality
      // This could trigger some background job or process.
      return NoContent();
    }
  }

  public class RssNewsItem
  {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Link { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Date { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
  }
}
