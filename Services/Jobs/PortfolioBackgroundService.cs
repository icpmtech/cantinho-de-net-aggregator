namespace MarketAnalyticHub.Services.Jobs
{
  using Hangfire;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Threading.Tasks;

  public class PortfolioBackgroundService
  {
    private readonly PortfolioService _portfolioService;
    private readonly PortfolioLossRuleService _ruleService;
    private readonly PushNotificationService _pushNotificationService;
    private PortfolioService portfolioService;

    public PortfolioBackgroundService(PortfolioService portfolioService)
    {
      this.portfolioService = portfolioService;
    }

    public PortfolioBackgroundService(PortfolioService portfolioService, PortfolioLossRuleService ruleService, PushNotificationService pushNotificationService)
    {
      _portfolioService = portfolioService;
      _ruleService = ruleService;
      _pushNotificationService = pushNotificationService;
    }

    // Method to check portfolio losses and send alerts based on dynamic rules
    public async Task CheckPortfolioLossesAsync()
    {
      var allUsers = await _portfolioService.GetAllUsersAsync(); // Retrieve all user IDs
      var rules = await _ruleService.GetAllRulesAsync(); // Retrieve all loss rules

      foreach (var user in allUsers)
      {
        var portfolios = await _portfolioService.GetPortfoliosByLossesUserAsync(user.UserId);
        foreach (var portfolio in portfolios)
        {
          var lossPercentage = portfolio.LossPercentage;
          foreach (var rule in rules)
          {
            if (lossPercentage >= rule.LossThreshold) // Check if portfolio loss meets or exceeds the rule threshold
            {
              await _portfolioService.SendPortfolioLossAlertAsync(portfolio.UserId, portfolio.CurrentMarketValue, lossPercentage);
              // Fetch the user's push subscription details
              var subscription = await _portfolioService.GetUserPushSubscriptionAsync(portfolio.UserId);
              if (subscription != null)
              {
                // Send push notification
                string title = "Portfolio Loss Alert";
                string message = $"Your portfolio has dropped by {lossPercentage}%!";
                string icon = "/assets/icons/marketanalytic_hub_icon_48x48.png"; // Update with the actual path to your icon
                string path = "/Dashboards"; // Update with the path to portfolio details
                var notificationPayload = new
                {
                  title = title,
                  body = message,
                  icon = icon,
                  path = path
                };
                string payloadJson = JsonSerializer.Serialize(notificationPayload);

                await _pushNotificationService.SendPushNotificationAsync(subscription, payloadJson);

              }

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
