using MarketAnalyticHub.Models.News;
using NewsAPI.Models;

namespace MarketAnalyticHub.Services
{
  public interface IMilvusService
  {
     Task<dynamic> TestConnectionAsync();
    Task StoreEmbeddingAsync(NewsItem article, float[] embedding);
    Task<bool> CreateMilvusCollectionNewsItemsAsync();
    Task<List<NewsItem>> SearchSimilarNewsAsync(float[] embedding, int topK = 5);
  }
}
