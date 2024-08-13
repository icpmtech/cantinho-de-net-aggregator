using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Services.Elastic;
using MarketAnalyticHub.Services.Jobs.Processors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Milvus.Client;
using NewsAPI.Models;
using System.Net.Http;
using System.Security.Claims;
using System.ServiceModel.Syndication;
using System.Xml;

namespace MarketAnalyticHub.Controllers.api
{



  [Authorize]
  [Route("api/[controller]")]
  [ApiController]
  public class NewsAnalizerController : ControllerBase
  {
    private readonly ElasticSearchService _elasticsearchService;
    private readonly HttpClient _httpClient;
    private readonly IArticleProcessor _articleProcessor;
    private readonly ILogger<NewsRSSApiController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly PortfolioService _portfolioService;
    private readonly IMilvusService _milvusService;
    
    public NewsAnalizerController(ElasticSearchService elasticSearchService,
      IMilvusService milvusService, PortfolioService portfolioService,HttpClient httpClient, IArticleProcessor articleProcessor, ApplicationDbContext context, ILogger<NewsRSSApiController> logger)
    {
      _elasticsearchService = elasticSearchService;
      _milvusService = milvusService;
      _httpClient = httpClient;
      _articleProcessor = articleProcessor;
      _logger = logger;
      _context = context;
    _portfolioService = portfolioService;
  }
    [HttpGet("analyze")]
    public async Task<IActionResult> AnalyzePortfolio([FromQuery] string[] stockSymbols)
    {
      var analysisResult = await _portfolioService.AnalyzePortfolioAsync(stockSymbols);
      return Ok(analysisResult);
    }
    [HttpGet("search-milvus")]
    public async Task<IActionResult> AnalyzePortfolio([FromQuery] string search)
    {
     
      return Ok("analysisResult");
    }
    [HttpGet("test-milvus-connection")]
    public async Task<dynamic> TestMilvusConnection()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
    return await  _articleProcessor.TestMilvusHealtyAsync();
        
    }
    [HttpGet("test-milvus-create-collection")]
    public async Task<IActionResult> CreateCollection()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      await _articleProcessor.TestCreateCollectionAsync();


      return Ok("Sucess!");
    }
    [HttpGet("test-search")]
    public async Task<IActionResult> Search(string  query)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      var newsArticles = await _elasticsearchService.SearchNewsItemsAsync(query,userId);
      if(newsArticles.Any())
      {
        var similarEvents = await _articleProcessor.ProcessSearchSimmilaryArticleAsync(newsArticles,userId);

        // Aggregate sentiment analysis
        var sentimentAnalysis = new Dictionary<string, string>();
        foreach (var article in newsArticles)
        {
          if (sentimentAnalysis.ContainsKey(article.Title))
          {
            sentimentAnalysis[article.Title] += article.SentimentAnalisys;
          }
          else
          {
            sentimentAnalysis[article.Title] = article.SentimentAnalisys;
          }
        }
       var analysis= new PortfolioAnalysisResult
        {
          Articles = newsArticles,
          SimilarEvents = similarEvents.ToList(),
          SentimentAnalysis = sentimentAnalysis // Sentiment data for charting
        };
        return Ok(analysis);
      }
      else
      {
        return Ok("No news in Search to analysis!");
      }
      
       


     
    }

    [HttpGet("test-milvus")]
    public async Task<IActionResult> TestProcessor()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      await _articleProcessor.ProcessArticleAsync(new NewsItem
      {
        Id = 1,
        Title = "Sample Title",
        Content = "Sample Content",
        Link = "https://example.com",
        Author = "Author Name",
        Category="Sample",
        Date = DateTime.Now.ToShortDateString(),
        Description = "Source Name"
      }, userId);
     

      return Ok("Sucess!");
    }

  }
}
