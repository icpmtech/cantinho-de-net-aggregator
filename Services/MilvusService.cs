namespace MarketAnalyticHub.Services
{
  using AspnetCoreMvcFull.Services;
  using Azure.Identity;
  using MarketAnalyticHub.Models.News;
  using Milvus.Client;

  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class MilvusService : IMilvusService
  {
    private readonly MilvusClient _milvusClient;

    public object MetricType { get; private set; }
   
    public MilvusService(IConfiguration configuration)
    {
      var zillizUri = configuration["Zilliz:Uri"];
      var apiKey = configuration["Zilliz:ApiKey"];
      _milvusClient = new MilvusClient(endpoint:new Uri(zillizUri),apiKey: apiKey);
    }
    public async Task<dynamic> TestConnectionAsync()
    {

      var healthState = await _milvusClient.HealthAsync();
        return healthState;
     
    }
    public async Task<bool> CreateMilvusCollectionNewsItemsAsync()
    {
      // Define the schema for the collection
      var schema = new CollectionSchema
      {
        Fields =
                {
                    FieldSchema.Create<long>("Id", isPrimaryKey: true),
                    FieldSchema.CreateVarchar("Title", maxLength: 1000),
                    FieldSchema.CreateVarchar("Link", maxLength: 1000),
                    FieldSchema.CreateVarchar("Author", maxLength: 200),
                    FieldSchema.CreateVarchar("Category", maxLength: 200),
                    FieldSchema.CreateVarchar("Date", maxLength: 200),
                    FieldSchema.CreateFloatVector("Description", dimension: 1536)
                }
      };

      // Create the collection with a specified number of shards
      var collection = await _milvusClient.CreateCollectionAsync("newsItems", schema, shardsNum: 2);

      if (collection != null)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public async Task StoreEmbeddingAsync(NewsItem article, float[] embedding)
    {
      // Prepare the data to be inserted
      // Convert embedding to the required format
      var floatVectorData = new List<ReadOnlyMemory<float>> { new ReadOnlyMemory<float>(embedding) };

      // Prepare the data to be inserted
      var entities = new List<FieldData>
    {
        new FieldData<long>("Id", new List<long> { (long)article.Id }),
        new FieldData<string>("Title", new List<string> { article.Title }),
        new FieldData<string>("Link", new List<string> { article.Link }),
        new FieldData<string>("Author", new List<string> { article.Author }),
        new FieldData<string>("Category", new List<string> { article.Category ?? "EMPTY_CATEGORY" }),
        new FieldData<string>("Date", new List<string> { article.Date }),
        new FloatVectorFieldData("Description", floatVectorData) // Correctly handle the float vector data
    };
      // Retrieve the collection
      var insertResult=await _milvusClient.GetCollection("newsItems").InsertAsync(entities);
     
      if (insertResult.InsertCount > 0)
      {
        Console.WriteLine("Embedding stored successfully.");
      }
      else
      {
        Console.WriteLine("Failed to store embedding.");
      }
    }

    public async Task<List<NewsItem>> SearchSimilarNewsAsync(float[] embedding, int topK = 5)
    {
      var parameters = new SearchParameters
      {
        ExtraParameters = { ["nprobe"] = "1024" },
        ConsistencyLevel = ConsistencyLevel.Strong,
        OutputFields = { "Id", "Title", "Link", "Author", "Category", "Date" }
      };
      var collection = _milvusClient.GetCollection("newsItems");
      var searchResults = await collection.SearchAsync(
        vectorFieldName: "Description",
        vectors: new ReadOnlyMemory<float>[] { embedding  },
        SimilarityMetricType.L2,
        limit: topK,
        parameters);
      // Process the search results and map them to NewsItem objects
      var similarNewsItems = new List<NewsItem>();

      foreach (var fieldData in searchResults.FieldsData)
      {
        var newsItem = new NewsItem();

      
          switch (fieldData.FieldName)
          {
            case "Id":
              newsItem.Id = (int?)((fieldData as FieldData<long>)?.Data.FirstOrDefault() ?? 0);
              break;
            case "Title":
              newsItem.Title = (fieldData as FieldData<string>)?.Data.FirstOrDefault() ?? string.Empty;
              break;
            case "Link":
              newsItem.Link = (fieldData as FieldData<string>)?.Data.FirstOrDefault() ?? string.Empty;
              break;
            case "Author":
              newsItem.Author = (fieldData as FieldData<string>)?.Data.FirstOrDefault() ?? string.Empty;
              break;
            case "Category":
              newsItem.Category = (fieldData as FieldData<string>)?.Data.FirstOrDefault() ?? string.Empty;
              break;
            case "Date":
              newsItem.Date = (fieldData as FieldData<string>)?.Data.FirstOrDefault() ?? string.Empty;
              break;
          }

        similarNewsItems.Add(newsItem);
      }

      return similarNewsItems;
    }

    
  }
}
