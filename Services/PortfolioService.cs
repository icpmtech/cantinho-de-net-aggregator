namespace AspnetCoreMvcFull.Services
{
  using AspnetCoreMvcFull.Models.Portfolio;
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class PortfolioService
  {
    private readonly ApplicationDbContext _context;

    public PortfolioService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByUserAsync(string userId)
    {
      var portfolios = await _context.Portfolios.Include(p => p.Items)
                                                .Where(p => p.UserId == userId)
                                                .ToListAsync();

      // Update current prices and calculate fields
      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          item.CurrentPrice = await GetCurrentPriceAsync(item.Symbol); // Implement this method to fetch the current price
        }
      }

      return portfolios;
    }

    public async Task<Portfolio> GetPortfolioByIdAsync(int id)
    {
      var portfolio = await _context.Portfolios.Include(p => p.Items)
                                                .FirstOrDefaultAsync(p => p.Id == id);

      if (portfolio != null)
      {
        foreach (var item in portfolio.Items)
        {
          item.CurrentPrice = await GetCurrentPriceAsync(item.Symbol); // Implement this method to fetch the current price
        }
      }

      return portfolio;
    }

    public async Task AddPortfolioAsync(Portfolio portfolio)
    {
      _context.Portfolios.Add(portfolio);
      await _context.SaveChangesAsync();
    }

    public async Task UpdatePortfolioAsync(Portfolio portfolio)
    {
      _context.Portfolios.Update(portfolio);
      await _context.SaveChangesAsync();
    }

    public async Task DeletePortfolioAsync(int id)
    {
      var portfolio = await _context.Portfolios.FindAsync(id);
      _context.Portfolios.Remove(portfolio);
      await _context.SaveChangesAsync();
    }

    private async Task<decimal> GetCurrentPriceAsync(string symbol)
    {
      // Implement logic to fetch the current price of the symbol from a market data API
      return await Task.FromResult(100m); // Dummy implementation
    }
  }

}
