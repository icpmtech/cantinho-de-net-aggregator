using MarketAnalyticHub.Models.News;

namespace MarketAnalyticHub.Services.Jobs.Processors
{
  public interface IArticleProcessor
  {
    Task<dynamic> TestMilvusHealtyAsync();
    Task<dynamic> TestCreateCollectionAsync();
    Task ProcessArticleAsync(NewsItem article, string userId);

    Task<IEnumerable<NewsItem>> ProcessSearchSimmilaryArticleAsync(IEnumerable<NewsItem> articles, string userId);
  }
}
