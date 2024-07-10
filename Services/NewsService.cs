namespace AspnetCoreMvcFull.Services
{
  using AspnetCoreMvcFull.Models.News;
  using System.Collections.Generic;
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
      return await _httpClient.GetFromJsonAsync<IEnumerable<NewsItem>>("/api/news/scraped");
    }
  }

}
