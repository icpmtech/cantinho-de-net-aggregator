
using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Services.Elastic;
using Nest;

namespace MarketAnalyticHub.Services
{
  public class DataIndexingService
  {
    private readonly ElasticSearchService _elasticSearchService;
    public DataIndexingService(ElasticSearchService elasticSearchService)
    {
      _elasticSearchService = elasticSearchService;
    }
    public async Task<ISearchResponse<PortfolioItem>> GetPortfolioValueAsync(int portfolioId)
    {
      var searchResponse = await _elasticSearchService._client.SearchAsync<PortfolioItem>(s => s
          .Query(q => q.Term(f => f.Id, portfolioId))
          .Aggregations(a => a
              .Sum("portfolio_value", sum => sum
                  .Field(f => f.Quantity * f.PurchasePrice) // Assuming the latest price is stored in PricePerUnit
              )
          )
      );

      return searchResponse;
    }
    public async Task<IEnumerable<PortfolioItem>> GetPortfolioItemsAsync(string userId)
    {
      var response = await _elasticSearchService._client.SearchAsync<PortfolioItem>(s => s
          .Index($"user-id-{userId}-portfolio")
          .Size(1000) // Adjust as needed
          .Query(q => q
              .Match(m => m
                  .Field(f => f.UserId)
                  .Query(userId)
              )
          )
      );

      if (!response.IsValid)
      {
        // Handle the error
        throw new Exception(response.OriginalException.Message);
      }

      return response.Documents;
    }
    public async Task<ISearchResponse<PortfolioItem>> GetPortfolioRiskAsync(int portfolioId)
    {
      var searchResponse = await _elasticSearchService._client.SearchAsync<PortfolioItem>(s => s
          .Query(q => q.Term(f => f.Id, portfolioId))
          .Aggregations(a => a
              .ExtendedStats("price_volatility", es => es
                  .Field(f => f.PurchasePrice)
              )
          )
      );

      return searchResponse;
    }
  }
}
