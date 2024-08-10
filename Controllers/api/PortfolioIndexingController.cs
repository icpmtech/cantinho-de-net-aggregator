using AspnetCoreMvcFull.Models;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers.api
{
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class PortfolioIndexingController : ControllerBase
  {
    private readonly PortfolioIndexingService _indexingService;

    public PortfolioIndexingController(PortfolioIndexingService indexingService)
    {
      _indexingService = indexingService;
    }

    [HttpPost("index")]
    public async Task<IActionResult> IndexData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      await _indexingService.IndexPortfolioDataAsync(userId);
      return Ok("Indexing completed");
    }
  }
}
