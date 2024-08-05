namespace MarketAnalyticHub.Controllers
{
  using MarketAnalyticHub.Services.ApiDataApp.Services;
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;

  [ApiController]
  [Route("api/[controller]")]
  public class SymbolsApiController : ControllerBase
  {
    private readonly ApiService _apiService;

    public SymbolsApiController(ApiService apiService)
    {
      _apiService = apiService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Get(string query)
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        return BadRequest("Keyword cannot be empty");
      }

      var data = await _apiService.GetApiDataAsync(query);
      return Ok(data);
    }
  }
}
