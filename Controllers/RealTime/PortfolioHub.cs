using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers.RealTime
{
  public class PortfolioHub : Hub
  {
    private readonly DataIndexingService _dataIndexingService;
    private readonly LlmService _llmService;
    private readonly ILogger<PortfolioHub> _logger;

    public PortfolioHub(DataIndexingService dataIndexingService, LlmService llmService, ILogger<PortfolioHub> logger)
    {
      _dataIndexingService = dataIndexingService;
      _llmService = llmService;
      _logger = logger;
    }

    public async Task SendPortfolioUpdatesMetrics(int portfolioId)
    {
      _logger.LogInformation("Received request to send portfolio metrics for Portfolio ID: {PortfolioId}", portfolioId);

      try
      {
        var metrics = await _dataIndexingService.GetPortfolioValueAsync(portfolioId);

        if (metrics != null && metrics.Aggregations != null)
        {
          await Clients.All.SendAsync("ReceivePortfolioUpdatesMetrics", metrics.Aggregations);
          _logger.LogInformation("Successfully sent portfolio metrics for Portfolio ID: {PortfolioId}", portfolioId);
        }
        else
        {
          _logger.LogWarning("No metrics found for Portfolio ID: {PortfolioId}", portfolioId);
          await Clients.Caller.SendAsync("ReceivePortfolioUpdatesMetrics", "No metrics available.");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while sending portfolio metrics for Portfolio ID: {PortfolioId}", portfolioId);
        await Clients.Caller.SendAsync("ReceivePortfolioUpdatesMetrics", "An error occurred while retrieving portfolio metrics. Please try again later.");
      }
    }

    public async Task SendPortfolioUpdatesInsights(int portfolioId)
    {
      _logger.LogInformation("Received request to send portfolio insights for Portfolio ID: {PortfolioId}", portfolioId);

      try
      {
        string prompt = $"Analyze the portfolio with ID {portfolioId} and provide insights.";
        var insights = await _llmService.GeneratePortfolioReportAsync(prompt);

        if (!string.IsNullOrWhiteSpace(insights))
        {
          await Clients.All.SendAsync("ReceivePortfolioUpdatesInsights", insights);
          _logger.LogInformation("Successfully sent portfolio insights for Portfolio ID: {PortfolioId}", portfolioId);
        }
        else
        {
          _logger.LogWarning("No insights generated for Portfolio ID: {PortfolioId}", portfolioId);
          await Clients.Caller.SendAsync("ReceivePortfolioUpdatesInsights", "No insights available.");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while sending portfolio insights for Portfolio ID: {PortfolioId}", portfolioId);
        await Clients.Caller.SendAsync("ReceivePortfolioUpdatesInsights", "An error occurred while generating portfolio insights. Please try again later.");
      }
    }
  }
}
