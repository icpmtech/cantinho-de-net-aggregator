namespace MarketAnalyticHub.Services
{
  using MarketAnalyticHub.Models.Portfolio;
  using MarketAnalyticHub.Models.Portfolio;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class PortfolioItemService
  {
    private readonly ApplicationDbContext _context;

    public PortfolioItemService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<PortfolioItem>> GetItemsByPortfolioAsync(int portfolioId)
    {
      var items = await _context.PortfolioItems.Where(p => p.PortfolioId == portfolioId).ToListAsync();

      // Update current prices
      foreach (var item in items)
      {
        item.CurrentPrice = await GetCurrentPriceAsync(item.Symbol); // Implement this method to fetch the current price
      }

      return items;
    }

    public async Task<PortfolioItem> GetItemByIdAsync(int id)
    {
      var item = await _context.PortfolioItems
                .Include(p => p.Industry)
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(m => m.Id == id);

      if (item != null)
      {
        item.CurrentPrice = await GetCurrentPriceAsync(item.Symbol); // Implement this method to fetch the current price
      }

      return item;
    }

    public async Task AddItemAsync(PortfolioItem item)
    {
      _context.PortfolioItems.Add(item);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(PortfolioItem item)
    {
      _context.PortfolioItems.Update(item);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
      var item = await _context.PortfolioItems.FindAsync(id);
      _context.PortfolioItems.Remove(item);
      await _context.SaveChangesAsync();
    }

    private async Task<decimal> GetCurrentPriceAsync(string symbol)
    {
      // Implement logic to fetch the current price of the symbol from a market data API
      return await Task.FromResult(100m); // Dummy implementation
    }

    public async Task<ActionResult<Transaction>> SellPortfolioItemAsync(int id, Transaction transaction)
    {
      var portfolioItem = await _context.PortfolioItems.FindAsync(id);

      if (portfolioItem == null)
      {
        return null;
      }

      // Assuming commission is stored per transaction and not per item
      // Calculate the revenue
      decimal grossProfit = transaction.Quantity * (transaction.SellPrice - portfolioItem.PurchasePrice);
      decimal totalCommission = (decimal)(portfolioItem.Commission + transaction.Commission);
      decimal revenue = grossProfit - totalCommission;

      transaction.Revenue = revenue;
      transaction.PortfolioItemId = id;

      _context.Transactions.Add(transaction);
      await _context.SaveChangesAsync();

      return transaction;
    }

  }

}
