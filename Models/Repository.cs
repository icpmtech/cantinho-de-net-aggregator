using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

    // Retrieve all rules, optionally filtered by UserProfileId
    public async Task<List<PortfolioLossRule>> GetAllRulesAsync(int? userProfileId = null)
    {
      if (userProfileId.HasValue)
      {
        return await _context.PortfolioLossRules
                             .Where(r => r.UserProfileId == userProfileId.Value)
                             .ToListAsync();
      }
      return await _context.PortfolioLossRules.ToListAsync();
    }

    // Retrieve a single rule by its Id
    public async Task<PortfolioLossRule> GetRuleByIdAsync(int id)
    {
      return await _context.PortfolioLossRules.FindAsync(id);
    }

    // Add a new rule
    public async Task AddRuleAsync(PortfolioLossRule rule)
    {
      _context.PortfolioLossRules.Add(rule);
      await _context.SaveChangesAsync();
    }

    // Update an existing rule
    public async Task UpdateRuleAsync(PortfolioLossRule rule)
    {
      _context.PortfolioLossRules.Update(rule);
      await _context.SaveChangesAsync();
    }

    // Delete a rule by its Id
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
