using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services.Elastic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;
using static MarketAnalyticHub.Controllers.SocialSentimentService;
using Field = YahooFinanceApi.Field;

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

    private async Task<decimal> GetLatestPriceForSymbolAsync(string symbol)
    {
      try
      {
        // Fetching the latest price from Yahoo Finance
        var securities = await Yahoo.Symbols(symbol).Fields(Field.Symbol, Field.RegularMarketPrice).QueryAsync();
        var security = securities[symbol];
        return (decimal)security.RegularMarketPrice;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to fetch the latest price for symbol {Symbol}", symbol);
        throw;
      }
    }

    public async Task<string> UpdatePortfolioPricesAsync(string userId)
    {
      var indexName = $"user-id-{userId}-portfolio";
      _logger.LogInformation("Starting price update for portfolio items for user {UserId} with index {IndexName}", userId, indexName);

      try
      {
        // Check if the index exists
        var indexExistsResponse = await _elasticClient._client.Indices.ExistsAsync(indexName);
        if (!indexExistsResponse.Exists)
        {
          var errorMessage = $"Index {indexName} does not exist. Cannot update prices.";
          _logger.LogError(errorMessage);
          return errorMessage;
        }

        // Fetch portfolio items for the user
        var portfolioItems = await _context.PortfolioItems
            .Where(pi => pi.Portfolio.UserId == userId)
            .ToListAsync();

        if (!portfolioItems.Any())
        {
          var warningMessage = $"No portfolio items found for user {userId}.";
          _logger.LogWarning(warningMessage);
          return warningMessage;
        }

        _logger.LogInformation("Found {ItemCount} portfolio items for user {UserId}.", portfolioItems.Count, userId);

        // Prepare bulk update descriptor
        var bulkUpdateDescriptor = new BulkDescriptor();

        foreach (var item in portfolioItems)
        {
          // Fetch the latest price for the item
          var latestPrice = await GetLatestPriceForSymbolAsync(item.Symbol);

          // Update the price in the portfolio item
          item.CurrentPrice = latestPrice;

          // Update the document in Elasticsearch
          bulkUpdateDescriptor.Update<PortfolioItem>(op => op
              .Index(indexName)
              .Id(item.Id)
              .Doc(item)
          );

          _logger.LogInformation("Prepared PortfolioItem ID {PortfolioItemId} for price update.", item.Id);
        }

        // Perform bulk update
        var bulkResponse = await _elasticClient._client.BulkAsync(bulkUpdateDescriptor);

        if (bulkResponse.Errors)
        {
          foreach (var itemWithError in bulkResponse.ItemsWithErrors)
          {
            _logger.LogError("Failed to update price for document ID {DocumentId}: {ErrorReason}", itemWithError.Id, itemWithError.Error.Reason);
          }

          return "Price update completed with errors.";
        }
        else
        {
          _logger.LogInformation("Successfully updated prices for portfolio items for user {UserId}.", userId);
          return "Price update completed successfully.";
        }
      }
      catch (Exception ex)
      {
        var errorMessage = $"An error occurred while updating prices for portfolio data for user {userId}: {ex.Message}";
        _logger.LogError(ex, errorMessage);
        return errorMessage;
      }
    }
    public async Task<string> IndexPortfolioDataAsync(string userId)
    {
      var indexName = $"user-id-{userId}-portfolio";
      _logger.LogInformation("Starting portfolio data indexing for user {UserId} with index {IndexName}", userId, indexName);

      try
      {
        // Check if the index exists, create if not
        var indexExistsResponse = await _elasticClient._client.Indices.ExistsAsync(indexName);
        if (!indexExistsResponse.Exists)
        {
          var createIndexResponse = await _elasticClient._client.Indices.CreateAsync(indexName, c => c
              .Map<PortfolioItem>(m => m.AutoMap())
          );

          if (!createIndexResponse.IsValid)
          {
            var errorMessage = $"Failed to create index {indexName}: {createIndexResponse.OriginalException.Message}";
            _logger.LogError(errorMessage);
            return errorMessage;
          }

          _logger.LogInformation("Index {IndexName} created successfully.", indexName);
        }

        
        // Fetch portfolio items for the user
        var portfolioItems = await _context.PortfolioItems
            .Where(pi => pi.Portfolio.UserId == userId)
            .ToListAsync();

        if (!portfolioItems.Any())
        {
          var warningMessage = $"No portfolio items found for user {userId}.";
          _logger.LogWarning(warningMessage);
          return warningMessage;
        }

        _logger.LogInformation("Found {ItemCount} portfolio items for user {UserId}.", portfolioItems.Count, userId);

        // Prepare bulk indexing
        var bulkIndexDescriptor = new BulkDescriptor();

        foreach (var item in portfolioItems)
        {
          // Fetch and set social sentiment data
          item.SocialSentiment = await GetSocialSentimentForSymbolAsync(item.Symbol);

          // Check if the document already exists
          var documentExistsResponse = await _elasticClient._client.DocumentExistsAsync<PortfolioItem>(item.Id, idx => idx.Index(indexName));

          if (documentExistsResponse.Exists)
          {
            // Document exists, update it
            bulkIndexDescriptor.Update<PortfolioItem>(op => op
                .Index(indexName)
                .Id(item.Id)
                .Doc(item)
                .DocAsUpsert(true) // Use DocAsUpsert to insert if it doesn't exist
            );

            _logger.LogInformation("Prepared PortfolioItem ID {PortfolioItemId} for update.", item.Id);
          }
          else
          {
            // Document does not exist, index it as new
            bulkIndexDescriptor.Index<PortfolioItem>(op => op
                .Index(indexName)
                .Document(item)
            );

            _logger.LogInformation("Prepared PortfolioItem ID {PortfolioItemId} for indexing.", item.Id);
          }
        }

        // Perform bulk indexing
        var bulkResponse = await _elasticClient._client.BulkAsync(bulkIndexDescriptor);

        if (bulkResponse.Errors)
        {
          foreach (var itemWithError in bulkResponse.ItemsWithErrors)
          {
            _logger.LogError("Failed to index/update document ID {DocumentId}: {ErrorReason}", itemWithError.Id, itemWithError.Error.Reason);
          }

          return "Indexing completed with errors.";
        }
        else
        {
          _logger.LogInformation("Successfully indexed/updated portfolio items for user {UserId}.", userId);
          return "Indexing completed successfully.";
        }
      }
      catch (Exception ex)
      {
        var errorMessage = $"An error occurred while indexing portfolio data for user {userId}: {ex.Message}";
        _logger.LogError(ex, errorMessage);
        return errorMessage;
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

    // Create an index
    public async Task<bool> CreateIndexAsync(string indexName)
    {
      var existsResponse = await _elasticClient._client.Indices.ExistsAsync(indexName);
      if (existsResponse.Exists)
      {
        _logger.LogWarning("Index {IndexName} already exists.", indexName);
        return false;
      }

      var createResponse = await _elasticClient._client.Indices.CreateAsync(indexName, c => c
          .Map(m => m.AutoMap())
      );

      if (createResponse.IsValid)
      {
        _logger.LogInformation("Index {IndexName} created successfully.", indexName);
        return true;
      }
      else
      {
        _logger.LogError("Failed to create index {IndexName}: {ErrorMessage}", indexName, createResponse.OriginalException.Message);
        return false;
      }
    }

    // Get a list of all indices
    public async Task<IReadOnlyCollection<CatIndicesRecord>> GetIndicesAsync()
    {
      var response = await _elasticClient._client.Cat.IndicesAsync();

      if (!response.IsValid)
      {
        // Handle errors here (throw exception, return empty list, log error, etc.)
        return new List<CatIndicesRecord>();
      }

      return response.Records;
    }

    // Delete an index
    public async Task<bool> DeleteIndexAsync(string indexName)
    {
      var deleteResponse = await _elasticClient._client.Indices.DeleteAsync(indexName);

      if (deleteResponse.IsValid)
      {
        _logger.LogInformation("Index {IndexName} deleted successfully.", indexName);
        return true;
      }
      else
      {
        _logger.LogError("Failed to delete index {IndexName}: {ErrorMessage}", indexName, deleteResponse.OriginalException.Message);
        return false;
      }
    }

    // Get details of an index
    public async Task<IndexState> GetIndexDetailsAsync(string indexName)
    {
      var response = await _elasticClient._client.Indices.GetAsync(indexName);
      if (response.Indices.ContainsKey(indexName))
      {
        return response.Indices[indexName];
      }
      else
      {
        _logger.LogWarning("Index {IndexName} not found.", indexName);
        return null;
      }
    }



  }
}
