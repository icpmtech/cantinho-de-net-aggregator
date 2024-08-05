using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Services;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class NewsController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly SentimentAnalysisService _sentimentAnalysisService;
    private readonly OpenAIService _openAIService;

    public NewsController(ApplicationDbContext context, SentimentAnalysisService sentimentAnalysisService,
      OpenAIService openAIService, HttpClient httpClient, ILogger<NewsController> logger)
    {
      _context = context;
      _sentimentAnalysisService = sentimentAnalysisService;
      _openAIService = openAIService;
      _httpClient = httpClient;
      _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeSentiment([FromBody] string text)
    {
      var result = await _sentimentAnalysisService.AnalyzeSentimentAsync(text);
      return Ok(result);
    }

    [HttpPost("generate-keywords")]
    public async Task<IActionResult> GenerateKeywords([FromBody] string description)
    {
      var keywords = await _openAIService.GenerateKeywordsAsync(description);
      return Ok(keywords);
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
        var newsItem = new NewsItem
        {
          Title = title,
          Link = link,
          Description = description,
          Author = author,
          Date = date??DateTime.Now.ToString(),
          Category = "Economy" // Use the category from the itemNews
        };

        // Analyze sentiment of the news title or description
        var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(newsItem.Description ?? newsItem.Title);
        newsItem.Sentiment = sentimentResult.Compound;
        var keywords =  await _openAIService.GenerateKeywordsAsync(newsItem.Description);

        newsItem.Keywords= keywords.ToList();
        var impact = await _openAIService.GenerateSentimentImpacts(newsItem.Description);
        newsItem.SentimentImpact= impact;
        var IndustriesImpact = await _openAIService.GenerateIndustryImpacts(newsItem.Description);
        newsItem.IndustriesImpact = IndustriesImpact;
        newsItems.Add(newsItem);
      }
      _context.News.AddRange(newsItems);
      _context.SaveChanges();
      return newsItems;
    }

    [HttpGet("scraped-dynamic")]
    public async Task<IEnumerable<NewsItem>> GetScrapedNewsDynamicAsync()
    {
      var newsScrapingItems = _context.NewsScrapingItem.ToList();
      var newsItems = new List<NewsItem>();

      foreach (var itemNews in newsScrapingItems)
      {
        var url = itemNews.Link;
        HttpResponseMessage response;

        try
        {
          response = await _httpClient.GetAsync(url);
          response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
          _logger.LogError(ex, $"Error fetching news from {url}.");
          continue;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"Fetched news page from {url}.");

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseBody);

        var newsNodes = htmlDoc.DocumentNode.SelectNodes(itemNews.TemplateScraping);

        if (newsNodes == null)
        {
          _logger.LogWarning($"No news items found on the fetched page from {url}.");
          continue;
        }

        foreach (var node in newsNodes)
        {
          var titleNode = node.SelectSingleNode(itemNews.TitleSelector);
          var linkNode = node.SelectSingleNode(itemNews.LinkSelector);
          var descriptionNode = node.SelectSingleNode(itemNews.DescriptionSelector);
          var authorNode = node.SelectSingleNode(itemNews.AuthorSelector);
          var dateNode = node.SelectSingleNode(itemNews.DateSelector);

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
            link = new Uri(new Uri(url), link).ToString();
          }

          var newsItem = new NewsItem
          {
            Title = title ?? "EMPTY",
            Link = link ?? "EMPTY",
            Description = description ?? "EMPTY",
            Author = author ?? "EMPTY",
            Date = date ?? DateTime.Now.ToString(),
            Category = itemNews.Category // Use the category from the itemNews
          };

          // Analyze sentiment of the news title or description
          var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(newsItem.Description ?? newsItem.Title);
          newsItem.Sentiment = sentimentResult.Compound;
          var keywords = await _openAIService.GenerateKeywordsAsync(newsItem.Description);
          newsItem.Keywords = keywords.ToList();
          var impact = await _openAIService.GenerateSentimentImpacts(newsItem.Description);
          newsItem.SentimentImpact = impact;
          var IndustriesImpact = await _openAIService.GenerateIndustryImpacts(newsItem.Description);
          newsItem.IndustriesImpact = IndustriesImpact;
          newsItems.Add(newsItem);
        }
      }
      _context.News.AddRange(newsItems);
      _context.SaveChanges();
      return newsItems;
    }


    [HttpPost("news-post")]
    public async Task<NewsItem> SaveNewsItem(NewsItemViewModel newsItemViewModel)
    {
     
      _logger.LogInformation("Save news item post.");

      var newsItem = new NewsItem
        {
          Title = newsItemViewModel.Title,
          Link = newsItemViewModel.Link,
          Description = newsItemViewModel.Description,
          Author = newsItemViewModel.Author,
          Date = newsItemViewModel.Date,
          Category = newsItemViewModel.Category  // Use the category from the itemNews
      };

        // Analyze sentiment of the news title or description
        var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(newsItem.Description ?? newsItem.Title);
        newsItem.Sentiment = sentimentResult.Compound;
        var keywords = await _openAIService.GenerateKeywordsAsync(newsItem.Description);
        newsItem.Keywords = keywords.ToList();
        var impact = await _openAIService.GenerateSentimentImpacts(newsItem.Description);
        newsItem.SentimentImpact = impact;
        var IndustriesImpact = await _openAIService.GenerateIndustryImpacts(newsItem.Description);
        newsItem.IndustriesImpact = IndustriesImpact;
      _context.News.Add(newsItem);
      _context.SaveChanges();
      return newsItem;
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
        var newsItem = new NewsItem
        {
          Title = title ?? "EMPTY",
          Link = link ?? "EMPTY",
          Description = description ?? "EMPTY",
          Author = author ?? "EMPTY",
          Date = date ?? DateTime.Now.ToString(),
          Category = "Energy and Environment"  // Use the category from the itemNews
        };

        // Analyze sentiment of the news title or description
        var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(newsItem.Description ?? newsItem.Title);
        newsItem.Sentiment = sentimentResult.Compound;
        var keywords = await _openAIService.GenerateKeywordsAsync(newsItem.Description);
        newsItem.Keywords = keywords.ToList();
        var impact = await _openAIService.GenerateSentimentImpacts(newsItem.Description);
        newsItem.SentimentImpact = impact;
        var IndustriesImpact = await _openAIService.GenerateIndustryImpacts(newsItem.Description);
        newsItem.IndustriesImpact = IndustriesImpact;
        newsItems.Add(newsItem);
      }
      _context.News.AddRange(newsItems);
      _context.SaveChanges();
      return newsItems;
    }

    [HttpPut("editnews/{id}")]
    public async Task<IActionResult> EditNews(int id, [FromBody] NewsItem newsItem)
    {
      if (id != newsItem.Id)
      {
        return BadRequest(new { success = false, message = "ID mismatch." });
      }

      if (ModelState.IsValid)
      {
        _context.News.Update(newsItem);
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "News updated successfully!" });
      }

      return BadRequest(new { success = false, message = "Invalid data received.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
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

    [HttpPost("get-associated-companies")]
    public async Task<IActionResult> GetAssociatedCompanies([FromBody] string[] keywords)
    {
      var companies = await _openAIService.GetAssociatedCompaniesAsync(keywords);
      var parsedCompanies = ParseResponse(companies);
      return Ok(new { companies = parsedCompanies });
    }
    private static object ParseResponse(string responseText)
    {
      var lines = responseText.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line));
      return lines.Select(line =>
      {
        var parts = line.Split('|').Select(part => part.Trim()).ToArray();
        return new
        {
          Name = parts.ElementAtOrDefault(0),
          Association = parts.ElementAtOrDefault(1),
          Sector = parts.ElementAtOrDefault(2),
          Market = parts.ElementAtOrDefault(3)
        };
      }).ToList();
    }
  }
}
