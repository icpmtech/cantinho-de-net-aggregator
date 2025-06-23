using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Models.Configurations;

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
  [HttpPost("CreateItemV1")]
  public async Task<IActionResult> CreateItemV1([FromBody] CreatePortfolioItemViewModel model)
  {
    // Validate model
    if (!ModelState.IsValid)
    {
      return BadRequest(ModelState);
    }

    // Map the ViewModel to the entity
    var newItem = new PortfolioItem
    {
      Symbol = model.Symbol,
      OperationType = model.OperationType,
      PortfolioId = model.PortfolioId,
      PurchaseDate = model.PurchaseDate ?? DateTime.Now,
      Quantity = model.Quantity,
      PurchasePrice = model.PurchasePrice,
      Commission = model.Commission,
      CurrentPrice = model.CurrentPrice
    };

    // Attach user from token
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    newItem.UserId = userId;

    // Save via service
    await _portfolioItemService.AddItemAsync(newItem);

    // Return the newly-created item info
    // If you have a GetItem action, you can reference it:
    return CreatedAtAction(
        nameof(GetItem),
        new { id = newItem.Id },
        newItem
    );
  }
  [HttpPut("UpdateItemV1/{id}")]
  public async Task<IActionResult> UpdateItemV1(int id, [FromBody] UpdatePortfolioItemViewModel model)
  {
    if (id != model.Id)
    {
      return BadRequest("Route ID and model ID do not match.");
    }

    // Retrieve the entity from the database (e.g., for concurrency checks)
    var existingItem = await _portfolioItemService.GetItemByIdAsync(id);
    if (existingItem == null)
    {
      return NotFound("Portfolio item not found.");
    }

    // Map the incoming ViewModel values onto the entity
    existingItem.Symbol = model.Symbol;
    existingItem.OperationType = model.OperationType;
    existingItem.PortfolioId = model.PortfolioId;
    existingItem.PurchaseDate = model.PurchaseDate??DateTime.MinValue;
    existingItem.Quantity = model.Quantity;
    existingItem.PurchasePrice = model.PurchasePrice;
    existingItem.Commission = model.Commission;
    existingItem.CurrentPrice = model.CurrentPrice;

    // The user ID could be attached from the token
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    existingItem.UserId = userId;

    // Perform the update
    await _portfolioItemService.UpdateItemAsync(existingItem);

    // return the updated entity (or a DTO)
    var updatedVm = new { id = existingItem.Id };
    return Ok(updatedVm);
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
