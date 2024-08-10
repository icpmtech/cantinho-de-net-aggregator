using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MarketAnalyticHub.Controllers.RealTime
{
  public class PortfolioHub : Hub
  {
    private readonly LlmService _llmService;
    private readonly DataIndexingService _dataIndexingService;
    public PortfolioHub(DataIndexingService dataIndexingService,LlmService llmService)
    {
      _llmService = llmService;
      _dataIndexingService = dataIndexingService;
    }
    public async Task SendPortfolioUpdatesMetrics(int portfolioId)
    {
      var metrics = await _dataIndexingService.GetPortfolioValueAsync(portfolioId);
      await Clients.All.SendAsync("ReceivePortfolioUpdatesMetrics", metrics.Aggregations);
    }
    public async Task SendPortfolioUpdatesInsights(int portfolioId)
    {
      // You can fetch portfolio data and use LLM to process it
      string prompt = $"Analyze the portfolio with ID {portfolioId} and provide insights.";
      var insights = await _llmService.GeneratePortfolioReportAsync(prompt);

      await Clients.All.SendAsync("ReceivePortfolioUpdatesInsights", insights);
    }
  }
}
