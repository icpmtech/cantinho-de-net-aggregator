using MarketAnalyticHub.Models;
using MarketAnalyticHub.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Services
{
  public class PortfolioLossRuleService
  {
    private readonly PortfolioLossRuleRepository _repository;

    public PortfolioLossRuleService(PortfolioLossRuleRepository repository)
    {
      _repository = repository;
    }

    // Retrieve all rules, optionally filtered by UserProfileId
    public async Task<List<PortfolioLossRule>> GetAllRulesAsync(int? userProfileId = null)
    {
      return await _repository.GetAllRulesAsync(userProfileId);
    }

    // Retrieve a single rule by its Id
    public async Task<PortfolioLossRule> GetRuleByIdAsync(int id)
    {
      return await _repository.GetRuleByIdAsync(id);
    }

    // Add a new rule
    public async Task AddRuleAsync(PortfolioLossRule rule)
    {
      await _repository.AddRuleAsync(rule);
    }

    // Update an existing rule
    public async Task UpdateRuleAsync(PortfolioLossRule rule)
    {
      await _repository.UpdateRuleAsync(rule);
    }

    // Delete a rule by its Id
    public async Task DeleteRuleAsync(int id)
    {
      await _repository.DeleteRuleAsync(id);
    }

    // Retrieve all rules associated with a specific user
    public async Task<List<PortfolioLossRule>> GetRulesByUserAsync(int userProfileId)
    {
      return await _repository.GetAllRulesAsync(userProfileId);
    }
  }
}
