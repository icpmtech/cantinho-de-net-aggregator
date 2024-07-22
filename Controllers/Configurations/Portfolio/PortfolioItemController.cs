using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;

[Route("api/[controller]")]
[Authorize]
public class PortfolioItemController : Controller
{
  private readonly PortfolioItemService _portfolioItemService;

  public PortfolioItemController(PortfolioItemService portfolioItemService)
  {
    _portfolioItemService = portfolioItemService;
  }

  [HttpGet("{portfolioId}")]
  public async Task<IActionResult> GetItems(int portfolioId)
  {
    var items = await _portfolioItemService.GetItemsByPortfolioAsync(portfolioId);
    return Ok(items);
  }

  [HttpGet("item/{id}")]
  public async Task<IActionResult> GetItem(int id)
  {
    var item = await _portfolioItemService.GetItemByIdAsync(id);
    if (item == null)
    {
      return NotFound();
    }
    return Ok(item);
  }

  [HttpPost]
  public async Task<IActionResult> AddItem([FromBody] PortfolioItem item)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    item.UserId = userId;
    await _portfolioItemService.AddItemAsync(item);
    return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateItem(int id, [FromBody] PortfolioItem item)
  {
    if (id != item.Id)
    {
      return BadRequest();
    }
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    item.UserId = userId;
    await _portfolioItemService.UpdateItemAsync(item);
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteItem(int id)
  {
    await _portfolioItemService.DeleteItemAsync(id);
    return NoContent();
  }
}
