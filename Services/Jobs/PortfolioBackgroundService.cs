namespace MarketAnalyticHub.Services.Jobs
{
  using Hangfire;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class PortfolioBackgroundService
  {
    private readonly PortfolioService _portfolioService;
    private readonly PortfolioLossRuleService _ruleService;
    private PortfolioService portfolioService;

    public PortfolioBackgroundService(PortfolioService portfolioService)
    {
      this.portfolioService = portfolioService;
    }

    public PortfolioBackgroundService(PortfolioService portfolioService, PortfolioLossRuleService ruleService)
    {
      _portfolioService = portfolioService;
      _ruleService = ruleService;
    }

    // Method to check portfolio losses and send alerts based on dynamic rules
    public async Task CheckPortfolioLossesAsync()
    {
      var allUsers = await _portfolioService.GetAllUsersAsync(); // Retrieve all user IDs
      var rules = await _ruleService.GetAllRulesAsync(); // Retrieve all loss rules

      foreach (var userId in allUsers)
      {
        var portfolios = await _portfolioService.GetPortfoliosByLossesUserAsync(userId.UserId);
        foreach (var portfolio in portfolios)
        {
          var lossPercentage = portfolio.LossPercentage;
          foreach (var rule in rules)
          {
            if (lossPercentage >= rule.LossThreshold) // Check if portfolio loss meets or exceeds the rule threshold
            {
              await _portfolioService.SendPortfolioLossAlertAsync(portfolio.UserId, portfolio.CurrentMarketValue, lossPercentage);
              break; // Optionally break after the first matching rule, depending on your logic
            }
          }
        }
      }
    }
  }


    public class Portfolio
  {
    public string UserId { get; set; }
    public decimal InitialValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal LossPercentage { get; set; } // New property to store the loss percentage
  }
}
