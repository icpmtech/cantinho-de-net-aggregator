using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class SentimentAnalyzerApiController : ControllerBase
  {
    private readonly PortfolioService _portfolioService;

    public SentimentAnalyzerApiController(PortfolioService portfolioService)
    {
      _portfolioService = portfolioService;
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
                                     e.Price
                                   })
                                 });

      return Ok(stockData);
    }
  }
}
