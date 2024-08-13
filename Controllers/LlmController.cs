using AspnetCoreMvcFull.Services;
using DocumentFormat.OpenXml.InkML;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Services.Elastic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Nest;
using static MarketAnalyticHub.Controllers.SocialSentimentService;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class LlmController : ControllerBase
  {
    private readonly LlmService _llmService;
    private readonly DataIndexingService _dataIndexingService;
    private readonly  ElasticSearchService _elasticSearchService;
    private readonly ApplicationDbContext _context;

   
    public LlmController(ApplicationDbContext context,ElasticSearchService elasticSearchService ,DataIndexingService dataIndexingService, LlmService llmService)
    {
      _llmService = llmService;
      _dataIndexingService = dataIndexingService;
      _elasticSearchService = elasticSearchService;
      _context = context;

    }
   

    [HttpGet("portfolio-value/{portfolioId}")]
    public async Task<IActionResult> GetPortfolioValue(int portfolioId)
    {
      var metrics = await _dataIndexingService.GetPortfolioValueAsync(portfolioId);
      return Ok(metrics.Aggregations);
    }
   


    [HttpPost("analyze-sentiment-for-stock")]
    public async Task<IActionResult> AnalyzeSentimentForStock(string tickerSymbol)
    {
      var prompt = $"Analyze the market sentiment for the stock {tickerSymbol}.";
      var sentiment = await _llmService.GeneratePortfolioReportAsync(prompt);
      return Ok(sentiment);
    }


    [HttpPost("investment-suggestions")]
    public async Task<IActionResult> GetInvestmentSuggestions(string portfolioSummary)
    {
      var prompt = $"Based on the following portfolio summary, suggest potential investment opportunities: {portfolioSummary}";
      var suggestions = await _llmService.GeneratePortfolioReportAsync(prompt);
      return Ok(suggestions);
    }

    [HttpPost("generate-report")]
    public async Task<IActionResult> GenerateReport([FromBody] string prompt)
    {
      var report = await _llmService.GeneratePortfolioReportAsync(prompt);
      return Ok(report);
    }
  }

}
