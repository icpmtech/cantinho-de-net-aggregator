using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using MarketAnalyticHub.Models.Portfolio;

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

  [HttpPost("{id}/sell")]
  public async Task<ActionResult<Transaction>> SellPortfolioItem(int id, Transaction transaction)
  {
    var portfolioItem = await _portfolioItemService.GetItemByIdAsync(id);

    if (portfolioItem == null)
    {
      return NotFound();
    }

    // Assuming commission is stored per transaction and not per item
    // Calculate the revenue
    decimal grossProfit = transaction.Quantity * (transaction.SellPrice - portfolioItem.PurchasePrice);
    decimal totalCommission = (decimal)(portfolioItem.Commission + transaction.Commission);
    decimal revenue = grossProfit - totalCommission;

    transaction.Revenue = revenue;
    transaction.PortfolioItemId = id;

    await _portfolioItemService.SellPortfolioItemAsync(id,transaction);

    return Ok(transaction);
  }


}
