namespace MarketAnalyticHub.Services.Elastic
{
  using Elasticsearch.Net;
  using Microsoft.Extensions.Configuration;
  using Nest;

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
  }
}
