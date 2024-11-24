
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MarketAnalyticHub.Services.ApiDataApp.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MarketAnalyticHub.Services
{
  public class ApiService 
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiService> _logger;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private const string ApiKey = "O831EHVCJBFEHUKE"; // Replace with your actual API key

    public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger, IMemoryCache cache)
    {
      _httpClientFactory = httpClientFactory;
      _logger = logger;
      _cache = cache;
    }

      // Existing methods...
      public async Task<dynamic> GetApiDataAsync(string keyword)
      {
        var queryUrl = $"https://www.alphavantage.co/query?function=SYMBOL_SEARCH&keywords={keyword}&apikey={ApiKey}";
        var response = await _httpClient.GetStringAsync(queryUrl);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(response);
        return jsonData;
      }
    
    public async Task<StockPriceResponse> GetStockPriceAsync(string symbol, string region)
    {
      if (string.IsNullOrWhiteSpace(symbol))
        throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));

      if (string.IsNullOrWhiteSpace(region))
        throw new ArgumentException("Region cannot be null or empty.", nameof(region));

      var cacheKey = $"StockPrice_{symbol}_{region}";

      if (_cache.TryGetValue(cacheKey, out StockPriceResponse cachedResponse))
      {
        _logger.LogInformation("Returning cached stock price for key: {CacheKey}", cacheKey);
        return cachedResponse;
      }

      var client = _httpClientFactory.CreateClient("YahooFinance");

      // Encode query parameters
      var encodedSymbol = Uri.EscapeDataString(symbol);
      var encodedRegion = Uri.EscapeDataString(region);
      var request = new HttpRequestMessage
      {
        Method = HttpMethod.Get,
        RequestUri = new Uri($"https://yahoo-finance166.p.rapidapi.com/api/stock/get-price?region={encodedRegion}&symbol={encodedSymbol}"),
        Headers =
  {
    { "x-rapidapi-key", "60c4b7c12emshfe21a6a1ec58d8bp1a396cjsn6bc88b0f02f5" },
    { "x-rapidapi-host", "yahoo-finance166.p.rapidapi.com" },
  },
      };
     
      try
      {
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
          var body = await response.Content.ReadAsStringAsync();

          var options = new JsonSerializerOptions
          {
            PropertyNameCaseInsensitive = true
          };

          var quoteSummary = JsonSerializer.Deserialize<QuoteSummary>(body, options);
          if (quoteSummary?.QuoteSummaryResult?.Result == null || quoteSummary.QuoteSummaryResult.Result.Count == 0)
          {
            _logger.LogError("Yahoo Finance API returned no results for symbol: {Symbol}", symbol);
            throw new HttpRequestException($"Yahoo Finance API returned no results for symbol: {symbol}");
          }

          var stockPriceResponse = new StockPriceResponse
          {
            Symbol = quoteSummary.QuoteSummaryResult.Result[0].Price.Symbol,
            Price = new PriceDetails
            {
              RegularMarketPrice = quoteSummary.QuoteSummaryResult.Result[0].Price.RegularMarketPrice.Raw,
              RegularMarketChange = quoteSummary.QuoteSummaryResult.Result[0].Price.RegularMarketChange.Raw,
              RegularMarketChangePercent = quoteSummary.QuoteSummaryResult.Result[0].Price.RegularMarketChangePercent.Raw,
              RegularMarketVolume = quoteSummary.QuoteSummaryResult.Result[0].Price.RegularMarketVolume.Raw
              // Map other properties as needed
            }
          };

          // Set cache options
          var cacheEntryOptions = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(30)); // Adjust as needed

          // Save data in cache
          _cache.Set(cacheKey, stockPriceResponse, cacheEntryOptions);

          return stockPriceResponse;
        }
        else
        {
          _logger.LogError("Yahoo Finance API returned error: {StatusCode} - {ReasonPhrase}",
                           response.StatusCode, response.ReasonPhrase);
          throw new HttpRequestException($"Yahoo Finance API Error: {response.StatusCode}");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while calling Yahoo Finance API for stock price.");
        throw; // Let the controller handle the exception
      }
    }
  }
}
