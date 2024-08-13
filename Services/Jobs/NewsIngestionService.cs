using MarketAnalyticHub.Services.Jobs.Processors;
using Newtonsoft.Json;

namespace MarketAnalyticHub.Services.Jobs
{
  public class NewsIngestionService : BackgroundService
  {
    private readonly HttpClient _httpClient;
    private readonly IArticleProcessor _articleProcessor;

    public NewsIngestionService(HttpClient httpClient, IArticleProcessor articleProcessor)
    {
      _httpClient = httpClient;
      _articleProcessor = articleProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      
    }
  }

}
