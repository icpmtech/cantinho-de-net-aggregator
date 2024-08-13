using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Services.Elastic;
using MarketAnalyticHub.Services.Jobs.Processors;
using NewsAPI.Models;
using System.Collections.Generic;

namespace MarketAnalyticHub.Services
{
  public class ArticleProcessor : IArticleProcessor
  {
    private readonly OpenAIService _openAIService;
    private readonly IMilvusService _milvusService;
    private readonly ElasticSearchService _elasticsearchService;

    public ArticleProcessor(OpenAIService openAIService, IMilvusService milvusService, ElasticSearchService elasticsearchService)
    {
      _openAIService = openAIService;
      _milvusService = milvusService;
      _elasticsearchService = elasticsearchService;
    }
    public async Task<dynamic> TestMilvusHealtyAsync()
    {
     
      return  await _milvusService.TestConnectionAsync();


    }
    public async Task ProcessArticleAsync(NewsItem article,string userId)
    {
      var embedding = await _openAIService.GenerateEmbeddingAsync(article.Title+article.Description);
      var sentiment = await _openAIService.AnalyzeSentimentAsync(article.Title + article.Description);
      
      if (embedding != null)
      {
        await _milvusService.StoreEmbeddingAsync(article, embedding);
        await _elasticsearchService.IndexNewsItemAsync(article, sentiment,userId);
      }
      
     
    }
    public async Task<IEnumerable<NewsItem>> ProcessSearchSimmilaryArticleAsync(IEnumerable<NewsItem> articles, string userId)
    {
      var result = new List<NewsItem>();

      foreach (var article in articles)
      {
        var embedding = await _openAIService.GenerateEmbeddingAsync(article.Title + article.Description);
        if (embedding != null)
        {
          var similarNews = await _milvusService.SearchSimilarNewsAsync(embedding);
          if (similarNews != null)
          {
            result.AddRange(similarNews);
          }
        }
      }

      return result;
    }


    public async Task<dynamic> TestCreateCollectionAsync()
    {
     return  await _milvusService.CreateMilvusCollectionNewsItemsAsync();
    }
  }

}
