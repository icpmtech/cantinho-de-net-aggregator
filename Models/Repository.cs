using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Repositories
{
  public class PortfolioLossRuleRepository
  {
    private readonly ApplicationDbContext _context;

    public PortfolioLossRuleRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<PortfolioLossRule>> GetAllRulesAsync()
    {
      return await _context.PortfolioLossRules.ToListAsync();
    }

    public async Task<PortfolioLossRule> GetRuleByIdAsync(int id)
    {
      return await _context.PortfolioLossRules.FindAsync(id);
    }

    public async Task AddRuleAsync(PortfolioLossRule rule)
    {
      _context.PortfolioLossRules.Add(rule);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateRuleAsync(PortfolioLossRule rule)
    {
      _context.PortfolioLossRules.Update(rule);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteRuleAsync(int id)
    {
      var rule = await _context.PortfolioLossRules.FindAsync(id);
      if (rule != null)
      {
        _context.PortfolioLossRules.Remove(rule);
        await _context.SaveChangesAsync();
      }
    }
  }
}
