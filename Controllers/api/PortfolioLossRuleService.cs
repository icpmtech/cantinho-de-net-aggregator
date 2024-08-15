
using MarketAnalyticHub.Services;

namespace MarketAnalyticHub.Controllers.api
{

  public class PortfolioCheckService
  {
    private readonly PortfolioLossRuleService _ruleService;
    private readonly PortfolioService _portfolioService;

    public PortfolioCheckService(PortfolioLossRuleService ruleService, PortfolioService portfolioService)
    {
      _ruleService = ruleService;
      _portfolioService = portfolioService;
    }

    public async Task CheckPortfolioLossesAsync()
    {
      var allUsers = await _portfolioService.GetAllUsersAsync();
      var rules = await _ruleService.GetAllRulesAsync();

      foreach (var userId in allUsers)
      {
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId.UserId);
        foreach (var portfolio in portfolios)
        {
          foreach (var rule in rules)
          {
            if (portfolio.LossPercentage >= rule.LossThreshold)
            {
              await _portfolioService.SendPortfolioLossAlertAsync(portfolio.UserId, portfolio.CurrentMarketValue, portfolio.LossPercentage);
            }
          }
        }
      }
    }
  }


  public class PortfolioLossRuleService
  {
    private readonly List<PortfolioLossRule> _rules = new List<PortfolioLossRule>();

    public Task<List<PortfolioLossRule>> GetAllRulesAsync()
    {
      return Task.FromResult(_rules);
    }

    public Task<PortfolioLossRule> GetRuleByIdAsync(int id)
    {
      var rule = _rules.FirstOrDefault(r => r.Id == id);
      return Task.FromResult(rule);
    }

    public Task AddRuleAsync(PortfolioLossRule rule)
    {
      rule.Id = _rules.Count > 0 ? _rules.Max(r => r.Id) + 1 : 1;
      _rules.Add(rule);
      return Task.CompletedTask;
    }

    public Task UpdateRuleAsync(PortfolioLossRule rule)
    {
      var existingRule = _rules.FirstOrDefault(r => r.Id == rule.Id);
      if (existingRule != null)
      {
        existingRule.LossThreshold = rule.LossThreshold;
        // Update other properties as needed
      }
      return Task.CompletedTask;
    }

    public Task DeleteRuleAsync(int id)
    {
      var rule = _rules.FirstOrDefault(r => r.Id == id);
      if (rule != null)
      {
        _rules.Remove(rule);
      }
      return Task.CompletedTask;
    }
  }

}
