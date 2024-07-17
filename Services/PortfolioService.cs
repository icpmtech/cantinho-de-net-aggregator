namespace AspnetCoreMvcFull.Services
{
  using AspnetCoreMvcFull.Models.Dashboard;
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
    public PortfolioStatisticsDto GetPortfolioStatistics()
    {
      // Fetch and calculate portfolio statistics
      var totalValue = 42820.00;
      var totalSymbols = 8258;
      var items = new List<PortfolioItemDto>
        {
            new PortfolioItemDto { Category = "Electronic", Description = "Mobile, Earbuds, TV", Value = 82500.00 },
            new PortfolioItemDto { Category = "Fashion", Description = "T-shirt, Jeans, Shoes", Value = 23800.00 },
            new PortfolioItemDto { Category = "Decor", Description = "Fine Art, Dining", Value = 849000.00 },
            new PortfolioItemDto { Category = "Sports", Description = "Football, Cricket Kit", Value = 9900.00 }
        };
      var chartData = new ChartDataDto
      {
        Labels = items.Select(i => i.Category).ToList(),
        Values = items.Select(i => i.Value).ToList()
      };

      return new PortfolioStatisticsDto
      {
        TotalValue = totalValue,
        TotalSymbols = totalSymbols,
        Items = items,
        ChartData = chartData
      };
    }

    public async Task<DashboardData> GetDashboardDataAsync(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);

      decimal totalInvestment = portfolios.Sum(p => p.TotalInvestment);
      decimal currentMarketValue = portfolios.Sum(p => p.CurrentMarketValue);
      decimal dividends = portfolios.Sum(p => p.Items.Sum(i => i.Dividends.Sum(d => d.Amount)));
      decimal profit = currentMarketValue - totalInvestment;
      decimal payments = 2456; // This should be replaced with actual payments data
      decimal operations = 14857; // This should be replaced with actual operations data
      decimal yearlyReport = 84686; // This should be replaced with actual yearly report data
      decimal growth = totalInvestment > 0 ? (profit / totalInvestment) * 100 : 0;
      decimal portfolioGrowth = totalInvestment > 0 ? (currentMarketValue / totalInvestment) * 100 : 0;

      return new DashboardData
      {
        Profit = profit,
        Dividends = dividends,
        Payments = payments,
        Operations = operations,
        TotalRevenue = currentMarketValue, // Example data
        Growth = growth,
        PortfolioGrowth = portfolioGrowth,
        YearlyReport = yearlyReport
      };
    }

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByUserAsync(string userId)
    {
      var portfolios = await _context.Portfolios
                                   .Include(p => p.Items)
                                   .ThenInclude(pi => pi.Dividends)
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
      var portfolio = await _context.Portfolios
                                   .Include(p => p.Items)
                                   .ThenInclude(pi => pi.Dividends)
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
