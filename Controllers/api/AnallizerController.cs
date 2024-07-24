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
using Microsoft.AspNetCore.Identity;
using MarketAnalyticHub.Models;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/analizer")]
  [ApiController]
  public class AnallizerController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnallizerController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly SentimentAnalysisService _sentimentAnalysisService;
    private readonly OpenAIService _openAIService;
    private readonly PortfolioService _portfolioService;
    private readonly UserManager<ApplicationUser> _userManager;
    public AnallizerController(UserManager<ApplicationUser> userManager ,ApplicationDbContext context, SentimentAnalysisService sentimentAnalysisService,
      OpenAIService openAIService, HttpClient httpClient, ILogger<AnallizerController> logger, PortfolioService portfolioService)
    {
      _userManager = userManager;
      _context = context;
      _sentimentAnalysisService = sentimentAnalysisService;
      _openAIService = openAIService;
      _httpClient = httpClient;
      _logger = logger;
      _portfolioService= portfolioService;
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

    [HttpPost("get-associated-companies")]
    public async Task<IActionResult> GetAssociatedCompanies([FromBody] KeywordsViewModel keywords)
    {
      var keys = keywords.Keywords.Split(',');
      var companies = await _openAIService.GetAssociatedCompaniesAsync(keys);
      var parsedCompanies = ParseResponse(companies);
      return Ok(new { companies = parsedCompanies });
    }

    [HttpGet("get-associated-companies/{newsId}")]
    public async Task<IActionResult> GetAssociatedCompanies(int newsId)
    {
      var keywords = GetKeywordsByNewsId(newsId);
      var table = await _openAIService.GetAssociatedCompaniesAsync(keywords.ToArray());
      return Ok(new { markdownTable = table });
    }

    [HttpPost("apply-sentiment-to-symbol")]
    public async Task<IActionResult> ApplySentimentToSymbol([FromBody] NewsSentimentRequest request)
    {
      // Step 1: Analyze sentiment
      var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(request.NewsText);

      // Step 2: Get associated symbols
      var keywords = request.Keywords.Split(',');
      var companiesResponse = await _openAIService.GetAssociatedCompaniesAsync(keywords);
      var companies = ParseResponse(companiesResponse);

      // Step 3: Create a mapping of symbols to sentiment
      var symbolSentiments = companies.Select(c => new SymbolSentiment
      {
        Symbol = c.Name, // Assuming the company name is used as the symbol
        Sentiment = sentimentResult.Score
      }).ToList();

      return Ok(symbolSentiments);
    }
    [HttpPost("analize-sentimentToPortfolio")]
    public async Task<IActionResult> MapSentimentToPortfolio(int newsId)
    {
      var userId = _userManager.GetUserId(User);
      // Assuming you have a method to get the news text by its ID
      var newsItem = await _context.News.FindAsync(newsId);
      if (newsItem == null)
      {
        return NotFound();
      }

      var response = await _portfolioService.MapSentimentToPortfolioAsync(userId, newsItem.Description);
      return new JsonResult(response);
    }
    private List<string> GetKeywordsByNewsId(int newsId)
    {
      var newsItem = _context.News.Where(s => s.Id == newsId).First();
      return newsItem.Keywords;
    }

    private static List<Company> ParseResponse(string responseText)
    {
      var lines = responseText.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
      var companies = new List<Company>();

      Company currentCompany = null;

      foreach (var line in lines)
      {
        if (line.Contains("Company name"))
        {
          if (currentCompany != null)
          {
            companies.Add(currentCompany);
          }

          currentCompany = new Company
          {
            Name = line.Replace("Company name", "").Trim()
          };
        }
        else if (line.Contains("Association:"))
        {
          if (currentCompany != null)
          {
            currentCompany.Association = line.Replace("Association:", "").Trim();
          }
        }
        else if (line.Contains("Sector:"))
        {
          if (currentCompany != null)
          {
            currentCompany.Sector = line.Replace("Sector:", "").Trim();
          }
        }
        else if (line.Contains("Market:"))
        {
          if (currentCompany != null)
          {
            currentCompany.Market = line.Replace("Market:", "").Trim();
          }
        }
      }

      if (currentCompany != null)
      {
        companies.Add(currentCompany);
      }

      return companies;
    }

    public class Company
    {
      public string Name { get; set; }
      public string Association { get; set; }
      public string Sector { get; set; }
      public string Market { get; set; }
    }

    public class SymbolSentiment
    {
      public string Symbol { get; set; }
      public double Sentiment { get; set; }
    }

    public class NewsSentimentRequest
    {
      public string NewsText { get; set; }
      public string Keywords { get; set; }
    }
  }
}
