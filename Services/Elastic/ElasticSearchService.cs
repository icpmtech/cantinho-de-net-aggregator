namespace MarketAnalyticHub.Services.Elastic
{
  using Elasticsearch.Net;
  using MarketAnalyticHub.Models.News;
  using MarketAnalyticHub.Models.Portfolio.Entities;
  using Microsoft.Extensions.Configuration;
  using Nest;
  using NewsAPI.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class ElasticSearchService
  {
    public readonly ElasticClient _client;

    public ElasticSearchService(IConfiguration configuration)
    {
      // Retrieve the connection parameters from the configuration
      var isCloud = bool.Parse(configuration["ElasticSearch:IsCloud"]);
      var node = configuration["ElasticSearch:Node"];
      
      var username = configuration["ElasticSearch:Username"];
      var password = configuration["ElasticSearch:Password"];

      // Create connection settings based on whether it's a cloud or local setup
      ConnectionSettings settings;

      if (isCloud)
      {
        var ES_URL = configuration["ElasticSearch:EndpoiPoint"];
        var API_KEY = configuration["ElasticSearch:ApiKey"];
        var cloudId = configuration["ElasticSearch:CloudId"];
        settings = new ConnectionSettings(cloudId,new ApiKeyAuthenticationCredentials(API_KEY))
            .DefaultIndex("web-articles");
      }
      else
      {
        settings = new ConnectionSettings(new Uri(node));

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
          settings.BasicAuthentication(username, password);
        }

        settings.DefaultIndex("web-articles");
      }

      _client = new ElasticClient(settings);
    }

    public async Task IndexNewsItemAsync(NewsItem article, string sentiment, string userId)
    {
      var indexName = $"user-id-{userId}-news-items";

      // Check if the index exists
      var indexExistsResponse = await _client.Indices.ExistsAsync(indexName);

      if (!indexExistsResponse.Exists)
      {
        // Create the index with specific mappings
        var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Map<NewsItem>(m => m
                .Properties(p => p
                    .Text(t => t.Name(n => n.Title))
                    .Text(t => t.Name(n => n.Description))
                    .Date(t => t.Name(n => n.Date).Format("yyyy-MM-dd")) // Specify the date format
                    .Text(t => t.Name(n => n.PublishedDate))
                    .Text(t => t.Name(n => n.Link))
                    .Text(t => t.Name(n => n.Author))
                    .Keyword(k => k.Name(n => n.SentimentAnalisys))
                // Add more fields if necessary
                )
            )
        );

        if (!createIndexResponse.IsValid)
        {
          throw new Exception($"Failed to create index {indexName}: {createIndexResponse.ServerError}");
        }
      }

      // Convert the date to a format Elasticsearch can parse
      DateTime parsedDate;
      if (DateTime.TryParseExact(article.Date, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
      {
        article.Date = parsedDate.ToString("yyyy-MM-dd"); // Convert to yyyy-MM-dd format
      }
      else
      {
        throw new Exception($"Failed to parse date: {article.Date}");
      }

      // Prepare the document to index
      var document = new
      {
        article.Title,
        article.Description,
        Date = article.Date, // Ensure date is in correct format
        article.PublishedDate,
        article.Link,
        article.Author,
        SentimentAnalisys = sentiment,
        UserId = userId,
        Timestamp = DateTime.UtcNow
      };

      // Index the document
      var indexResponse = await _client.IndexAsync(document, i => i.Index(indexName));

      if (!indexResponse.IsValid)
      {
        throw new Exception($"Failed to index document in {indexName}: {indexResponse.ServerError}");
      }
    }
    public async Task<IEnumerable<NewsItem>> SearchNewsItemsAsync(string query, string userId)
    {
      var indexName = $"user-id-{userId}-news-items";

      // Prepare the search request
      var searchResponse = await _client.SearchAsync<NewsItem>(s => s
          .Index(indexName)
          .Query(q => q
              .MultiMatch(m => m
                  .Query(query)
                  .Fields(f => f
                      .Field(p => p.Title)
                      .Field(p => p.Description)
                      .Field(p => p.Author)
                      .Field(p => p.SentimentAnalisys)
                  )
              )
          )
      );

      if (!searchResponse.IsValid)
      {
        throw new Exception($"Failed to search documents in {indexName}: {searchResponse.ServerError}");
      }

      // Return the matching documents
      return searchResponse.Documents;
    }
   
  }
}
