namespace MarketAnalyticHub.Services.Jobs
{
  using Hangfire;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class PortfolioBackgroundService
  {
    private readonly PortfolioService _portfolioService;

    public PortfolioBackgroundService(PortfolioService portfolioService)
    {
      _portfolioService = portfolioService;
    }

    // Method to check portfolio losses and send alerts
    public async Task CheckPortfolioLossesAsync()
    {
      var allUsers = await _portfolioService.GetAllUsersAsync(); // Assuming this method exists to retrieve all user IDs

      foreach (var userId in allUsers)
      {
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId.UserId);
        foreach (var portfolio in portfolios)
        {
          var lossPercentage = portfolio.LossPercentage;
          if (lossPercentage >= 0)
          {
            await _portfolioService.SendPortfolioLossAlertAsync(portfolio.UserId, portfolio.CurrentMarketValue, lossPercentage);
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
