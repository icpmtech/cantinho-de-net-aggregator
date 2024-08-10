using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services.Elastic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using static MarketAnalyticHub.Controllers.SocialSentimentService;

namespace MarketAnalyticHub.Services
{
  public class PortfolioIndexingService
  {
    private readonly ApplicationDbContext _context;
    private readonly ElasticSearchService _elasticClient;
    private readonly ILogger<PortfolioIndexingService> _logger;

    public PortfolioIndexingService(ApplicationDbContext context, ElasticSearchService elasticClient, ILogger<PortfolioIndexingService> logger)
    {
      _context = context;
      _elasticClient = elasticClient;
      _logger = logger;
    }

    public async Task IndexPortfolioDataAsync(string userId)
    {
      var indexName = $"user-id-{userId}-portfolio";
      _logger.LogInformation("Starting portfolio data indexing for user {UserId} with index {IndexName}", userId, indexName);

      try
      {
        // Check if the index exists and create it if not
        var indexExistsResponse = await _elasticClient._client.Indices.ExistsAsync(indexName);
        if (!indexExistsResponse.Exists)
        {
          var createIndexResponse = await _elasticClient._client.Indices.CreateAsync(indexName, c => c
              .Map<PortfolioItem>(m => m.AutoMap())
          );

          if (!createIndexResponse.IsValid)
          {
            _logger.LogError("Failed to create index {IndexName}: {ErrorMessage}", indexName, createIndexResponse.OriginalException.Message);
            return;
          }

          _logger.LogInformation("Index {IndexName} created successfully.", indexName);
        }

        // Include related entities and filter by userId
        var portfolioItems = await _context.PortfolioItems
            .Include(pi => pi.Portfolio)
            .Include(pi => pi.Industry)
            .Include(pi => pi.Dividends)
            .Include(pi => pi.StockEvents)
            .Where(pi => pi.Portfolio.UserId == userId)
            .ToListAsync();

        if (!portfolioItems.Any())
        {
          _logger.LogWarning("No portfolio items found for user {UserId}.", userId);
          return;
        }

        _logger.LogInformation("Found {ItemCount} portfolio items for user {UserId}.", portfolioItems.Count, userId);

        // Prepare items for bulk indexing
        var bulkIndexDescriptor = new BulkDescriptor();

        foreach (var item in portfolioItems)
        {
          // Simulate fetching social sentiment for each item (replace with your implementation)
          item.SocialSentiment = await GetSocialSentimentForSymbolAsync(item.Symbol);

          // Add the item to the bulk index descriptor
          bulkIndexDescriptor.Index<PortfolioItem>(op => op
              .Index(indexName)
              .Document(item)
          );

          _logger.LogInformation("Prepared PortfolioItem ID {PortfolioItemId} for indexing.", item.Id);
        }

        // Perform bulk indexing
        var bulkResponse = await _elasticClient._client.BulkAsync(bulkIndexDescriptor);

        if (bulkResponse.Errors)
        {
          foreach (var itemWithError in bulkResponse.ItemsWithErrors)
          {
            _logger.LogError("Failed to index document ID {DocumentId}: {ErrorReason}", itemWithError.Id, itemWithError.Error.Reason);
          }
        }
        else
        {
          _logger.LogInformation("Successfully indexed portfolio items for user {UserId}.", userId);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while indexing portfolio data for user {UserId}.", userId);
      }
    }

    private async Task<SocialSentiment> GetSocialSentimentForSymbolAsync(string symbol)
    {
      // Replace with actual logic to fetch social sentiment
      _logger.LogInformation("Fetching social sentiment for symbol {Symbol}.", symbol);
      return await Task.FromResult(new SocialSentiment
      {
        Symbol = symbol,
        SentimentScore = 0.5 // Placeholder value
      });
    }
  }
}
