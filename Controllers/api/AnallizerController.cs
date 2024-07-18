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

    public AnallizerController(ApplicationDbContext context, SentimentAnalysisService sentimentAnalysisService,
      OpenAIService openAIService,  HttpClient httpClient, ILogger<AnallizerController> logger)
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
      // Fetch keywords for the newsId. This part is pseudo-code and needs to be replaced
      // with actual logic to fetch keywords based on the newsId.
      var keywords =  GetKeywordsByNewsId(newsId);

      var table = await _openAIService.GetAssociatedCompaniesAsync(keywords.ToArray());
      return Ok(new { markdownTable = table });
    }
    private  List<string> GetKeywordsByNewsId(int newsId)
    {
      // Replace this with your actual logic to fetch keywords by newsId
      // Example:
      var newsItem =  _context.News.Where(s=>s.Id==newsId).First();
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
  }
}
