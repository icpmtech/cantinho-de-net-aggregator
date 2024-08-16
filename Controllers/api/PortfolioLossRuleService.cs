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

    public async Task<List<PortfolioLossRule>> GetAllRulesAsync()
    {
      return await _repository.GetAllRulesAsync();
    }

    public async Task<PortfolioLossRule> GetRuleByIdAsync(int id)
    {
      return await _repository.GetRuleByIdAsync(id);
    }

    public async Task AddRuleAsync(PortfolioLossRule rule)
    {
      await _repository.AddRuleAsync(rule);
    }

    public async Task UpdateRuleAsync(PortfolioLossRule rule)
    {
      await _repository.UpdateRuleAsync(rule);
    }

    public async Task DeleteRuleAsync(int id)
    {
      await _repository.DeleteRuleAsync(id);
    }
  }
}
