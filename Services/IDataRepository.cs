using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;


namespace MarketAnalyticHub.Repositories
{
  public interface IDataRepository
  {
    Task<IEnumerable<Portfolio>> GetPortfoliosByUserAsync(string userId);
    
    Task<Portfolio> GetPortfolioByIdAsync(int portfolioId);
  
    Task<IEnumerable<PortfolioItem>> GetPortfolioItemsByPortfolioIdAsync(int portfolioId);
    Task<PortfolioItem?> GetPortfolioItemByIdAsync(int itemId);
  }
  public class DataRepository : IDataRepository
  {
    private readonly ApplicationDbContext _dbContext;

    public DataRepository(ApplicationDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByUserAsync(string userId)
    {
      return await _dbContext.Portfolios
          .Include(p => p.Items)
          .ThenInclude(item => item.Dividends)
          .Where(p => p.UserId == userId)
          .ToListAsync();
    }



    public async Task<Portfolio> GetPortfolioByIdAsync(int portfolioId)
    {
      return await _dbContext.Portfolios
          .Include(p => p.Items)
          .ThenInclude(item => item.Dividends)
          .FirstOrDefaultAsync(p => p.Id == portfolioId);
    }



    public async Task<IEnumerable<PortfolioItem>> GetPortfolioItemsByPortfolioIdAsync(int portfolioId)
    {
      return await _dbContext.PortfolioItems
          .Include(item => item.Dividends)
          .Where(item => item.PortfolioId == portfolioId)
          .ToListAsync();
    }

    public async Task<PortfolioItem?> GetPortfolioItemByIdAsync(int itemId)
    {
      return await _dbContext.PortfolioItems
          .Include(item => item.Dividends)
          .FirstOrDefaultAsync(item => item.Id == itemId);
    }
  }

}



