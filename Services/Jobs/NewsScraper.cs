using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Services;
using HtmlAgilityPack;
using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MarketAnalyticHub.Services.Jobs
{
  public class NewsScraper
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NewsScraper> _logger;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly SentimentAnalysisService _sentimentAnalysisService;
    private readonly OpenAIService _openAIService;

    public NewsScraper(
        IHttpClientFactory httpClientFactory,
        ILogger<NewsScraper> logger,
        IConfiguration configuration,
        SentimentAnalysisService sentimentAnalysisService,
        ApplicationDbContext context,
        OpenAIService openAIService)
    {
      _httpClientFactory = httpClientFactory;
      _logger = logger;
      _sentimentAnalysisService = sentimentAnalysisService;
      _context = context;
      _openAIService = openAIService;
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

    public async Task ScrapeNewsAsync(int id)
    {
      var newsScrapingItems = await _context.NewsScrapingItem.Where(q => q.Id == id).ToListAsync();
      var newsItems = new List<NewsItem>();

      foreach (var itemNews in newsScrapingItems)
      {
        var url = itemNews.Link;
        HttpResponseMessage response;

        try
        {
          var httpClient = _httpClientFactory.CreateClient();
          response = await httpClient.GetAsync(url);
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
            Date = date??DateTime.Now.ToString(),
            Category = itemNews.Category // Use the category from the itemNews
          };

          // Analyze sentiment of the news title or description
          if (newsItem.Title!="EMPTY" && newsItem.Description != "EMPTY") {
          var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(newsItem.Description ?? newsItem.Title);
          newsItem.Sentiment = sentimentResult.Compound;
          var keywords = await _openAIService.GenerateKeywordsAsync(newsItem.Description);
          newsItem.Keywords = keywords.ToList();
           
         
            var impact = await _openAIService.GenerateSentimentImpacts(newsItem.Description);
          newsItem.SentimentImpact = impact;

          var industriesImpact = await _openAIService.GenerateIndustryImpacts(newsItem.Description);
          newsItem.IndustriesImpact = industriesImpact;
          newsItems.Add(newsItem);
          }

         
        }
      }

      _context.News.AddRange(newsItems);
      await _context.SaveChangesAsync();
    }
  }
}
