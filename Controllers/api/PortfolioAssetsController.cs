using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  public class AssetsController : ControllerBase
  {
    private readonly IPortfolioAssetsService _portfolioService;

    public AssetsController(IPortfolioAssetsService portfolioService)
    {
      _portfolioService = portfolioService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssets([FromQuery] string sortBy = "value")
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      var assets = await _portfolioService.GetAssetsAsync(userId,sortBy);
      return Ok(assets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetDto>> GetAsset(int id)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      var assets = await _portfolioService.GetAssetsAsync(userId);
      var asset = assets.FirstOrDefault(a => a.Id == id);

      if (asset == null)
      {
        return NotFound();
      }

      return Ok(asset);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<PortfolioSummaryDto>> GetSummary([FromQuery] string period = "today")
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      var summary = await _portfolioService.GetPortfolioSummaryAsync( userId, period);
      return Ok(summary);
    }


  }

 

  
}
