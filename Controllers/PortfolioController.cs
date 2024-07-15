using AspnetCoreMvcFull.Models.Portfolio;
using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[Authorize]
public class PortfolioController : Controller
{
  private readonly PortfolioService _portfolioService;

  public PortfolioController(PortfolioService portfolioService)
  {
    _portfolioService = portfolioService;
  }

  [HttpGet]
  public async Task<IActionResult> GetPortfolios()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
    return Ok(portfolios);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetPortfolio(int id)
  {
    var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
    if (portfolio == null)
    {
      return NotFound();
    }
    return Ok(portfolio);
  }

  [HttpPost]
  public async Task<IActionResult> AddPortfolio([FromBody] Portfolio portfolio)
  {
    portfolio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    await _portfolioService.AddPortfolioAsync(portfolio);
    return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] Portfolio portfolio)
  {
    if (id != portfolio.Id)
    {
      return BadRequest();
    }
    await _portfolioService.UpdatePortfolioAsync(portfolio);
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeletePortfolio(int id)
  {
    await _portfolioService.DeletePortfolioAsync(id);
    return NoContent();
  }
}
