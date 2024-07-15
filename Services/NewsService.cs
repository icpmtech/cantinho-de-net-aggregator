namespace MarketAnalyticHub.Services
{
  using MarketAnalyticHub.Models.News;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Threading.Tasks;

  public class NewsService
  {
    private readonly HttpClient _httpClient;

    public NewsService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<IEnumerable<NewsItem>> GetNewsAsync()
    {
      var newsTask1 = _httpClient.GetFromJsonAsync<IEnumerable<NewsItem>>("/api/news/scraped-env");
      var newsTask2 = _httpClient.GetFromJsonAsync<IEnumerable<NewsItem>>("/api/news/scraped");

      await Task.WhenAll(newsTask1, newsTask2);

      var news1 = await newsTask1;
      var news2 = await newsTask2;

      return news1.Concat(news2);
    }
  }
}
