using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Models.SetupDb;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class SentimentAnalyzerApiController : ControllerBase
  {
    private readonly PortfolioService _portfolioService;
    private readonly OpenAIService _openAIService;
    public SentimentAnalyzerApiController(PortfolioService portfolioService, OpenAIService openAIService)
    {
      _portfolioService = portfolioService;

      _openAIService = openAIService;
   
    }

    [HttpGet("GetStockData")]
    public async Task<IActionResult> GetStockData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

      var stockData = portfolios.SelectMany(p => p.Items)
                                 .Select(s => new
                                 {
                                   s.Symbol,
                                   StockEvents = s?.StockEvents?.OrderBy(e => e.Date).Select(e => new
                                   {
                                     e.Date,
                                     e.Impact,
                                     e.Sentiment,
                                     e.Id,
                                     e.PortfolioItemId,
                                     e.Price,
                                     e.EventName,
                                     e.Source,
                                     e.PriceChange
                                   })
                                 });

      return Ok(stockData);
    }

    [HttpGet("analise-score-impact")]
    public async Task<IActionResult> GetScoreImpact(string inputTextToAnalise, string symbol)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

      // Perform the analysis
      var analysisResult = await _openAIService.GenerateScoreImpacts(inputTextToAnalise, symbol);

      // Return the analysis result
      return Ok(new { Analysis = analysisResult });
    }

  }
}
