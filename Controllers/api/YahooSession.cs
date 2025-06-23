using Flurl;
using Flurl.Http;
using MarketAnalyticHub.Models.YahooFinance;
using MarketAnalyticHub.Models.Yhaoo.Models;
using MarketAnalyticHub.YaooServive.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace MarketAnalyticHub.Controllers.api
{
  internal static partial class YahooService
  {
    private static string _crumb;
    private static FlurlCookie _cookie;
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public const string UserAgentKey = "User-Agent";
    public const string UserAgentValue = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0";

    public static string Crumb => _crumb;
    public static FlurlCookie Cookie => _cookie;

    public static async Task InitAsync(CancellationToken token = default)
    {
      if (_crumb != null)
      {
        return;
      }

      await _semaphore.WaitAsync(token).ConfigureAwait(false);
      try
      {
        var response = await "https://fc.yahoo.com"
            .AllowHttpStatus("404")
            .AllowHttpStatus("500")
            .AllowHttpStatus("502")
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync()
            .ConfigureAwait(false);

        _cookie = response.Cookies.FirstOrDefault(c => c.Name == "A3");

        if (_cookie == null)
        {
          throw new Exception("Failed to obtain Yahoo auth cookie.");
        }
        else
        {
          _cookie = response.Cookies[0];

          _crumb = await "https://query1.finance.yahoo.com/v1/test/getcrumb"
              .WithCookie(_cookie.Name, _cookie.Value)
              .WithHeader(UserAgentKey, UserAgentValue)
              .GetAsync(token)
              .ReceiveString();

          if (string.IsNullOrEmpty(_crumb))
          {
            throw new Exception("Failed to retrieve Yahoo crumb.");
          }
        }
      }
      finally
      {
        _semaphore.Release();
      }
    }

    public static async Task<dynamic> GetCashFlowAsync(string symbol, CancellationToken token = default)
    {
      await InitAsync(token);

      var url = $"https://query1.finance.yahoo.com/v10/finance/quoteSummary/{symbol}?modules=cashflowStatementHistory,cashflowStatementHistoryQuarterly";

      try
      {
        var response = await url.SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var cashFlow = response.quoteSummary.result;
        if (cashFlow == null)
        {
          throw new Exception($"Failed to retrieve cash flow data for symbol: {symbol}");
        }

        return cashFlow;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetCashFlowAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<dynamic> GetCurrentPriceAsync(string symbol, CancellationToken token = default)
    {
      await InitAsync(token);

      var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={symbol}";
      
      try
      {
        var response = await url.SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var quote = response.quoteResponse.result;
        if (quote == null)
        {
          throw new Exception($"Failed to retrieve quote for symbol: {symbol}");
        }

        return response.quoteResponse.result;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetCurrentPriceAsync: {ex.Message}");
        throw;
      }
    }
    public static async Task<Dictionary<string, decimal>> GetQuotesAsync(string symbol, CancellationToken token = default)
    {
      await InitAsync(token);

      var symbolList = string.Join(",", symbol);
      var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={symbolList}";
      try
      {
        var response = await url.SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var quotes = response.quoteResponse.result.ToObject<List<Quote>>();

        var dictionary = quotes.ToDictionary();

        return dictionary;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetMultipleQuotesAsync: {ex.Message}");
        throw;
      }
    }
    public static async Task<Dictionary<string, decimal>> GetMultipleQuotesAsync(List<string> symbols, CancellationToken token = default)
    {
      await InitAsync(token);

      var symbolList = string.Join(",", symbols);
      var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={symbolList}";
      try
      {
        var response = await url.SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var quotes = response.quoteResponse.result.ToObject<List<Quote>>();

        var dictionary = quotes.ToDictionary();

        return dictionary;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetMultipleQuotesAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<dynamic> GetBalanceSheetAsync(string symbol, CancellationToken token = default)
    {
      await InitAsync(token);

      var url = $"https://query1.finance.yahoo.com/v10/finance/quoteSummary/{symbol}?modules=balanceSheetHistory,balanceSheetHistoryQuarterly";
      try
      {
        var response = await url.SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var balanceSheet = response.quoteSummary.result;
        if (balanceSheet == null)
        {
          throw new Exception($"Failed to retrieve balance sheet for symbol: {symbol}");
        }

        return balanceSheet;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetBalanceSheetAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<dynamic> GetStockDetailsAsync(string symbol, CancellationToken token = default)
    {
      await InitAsync(token);

      var url = $"https://query1.finance.yahoo.com/v10/finance/quoteSummary/{symbol}?modules=summaryProfile,financialData,defaultKeyStatistics";
      try
      {
        var response = await url.SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var details = response.quoteSummary.result;
        if (details == null)
        {
          throw new Exception($"Failed to retrieve details for symbol: {symbol}");
        }

        return details;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetStockDetailsAsync: {ex.Message}");
        throw;
      }
    }
    public class HistoricalData
    {
      public long Timestamp { get; set; }
      public decimal Open { get; set; }
      public decimal High { get; set; }
      public decimal Low { get; set; }
      public decimal Close { get; set; }
      public long Volume { get; set; }
    }
    /// <summary>
    /// Fetches the industry information for a given stock symbol.
    /// </summary>
    /// <param name="symbol">The stock symbol.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The industry associated with the symbol.</returns>
    public static async Task<string> GetIndustryBySymbolAsync(string symbol, CancellationToken token = default)
    {
      return "none";
      //if (string.IsNullOrWhiteSpace(symbol))
      //  throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));

      //// Initialize necessary components
      //await InitAsync(token);

      //// Construct the URL
      //var url = $"https://query2.finance.yahoo.com/v10/finance/quoteSummary/{Uri.EscapeDataString(symbol)}?modules=assetProfile";

      //try
      //{
      //  var response = await url
      //      .SetQueryParam("crumb", Crumb)
      //      .WithCookie(_cookie.Name, _cookie.Value)
      //      .WithHeader(UserAgentKey, UserAgentValue)
      //      .GetAsync(token)
      //      .ReceiveJson<QuoteSummaryResponse>();
      //  Thread.Sleep(100);
      //  var industry = response?.QuoteSummary?.Result?[0]?.AssetProfile?.Industry;

      //  if (string.IsNullOrEmpty(industry))
      //  {
      //    throw new InvalidOperationException($"Industry information not found for symbol: {symbol}");
      //  }

      //  return industry;
      //}
      //catch (FlurlHttpException ex)
      //{
      //  var errorResponse = await ex.GetResponseStringAsync();
      //  Console.WriteLine($"Flurl HTTP Error in GetIndustryBySymbolAsync: {ex.Message}\nResponse: {errorResponse}");
      //  throw new HttpRequestException("Error fetching industry information from Yahoo Finance.", ex);
      //}
      //catch (Exception ex)
      //{
      //  Console.WriteLine($"Unexpected Error in GetIndustryBySymbolAsync: {ex.Message}");
      //  throw;
      //}
    }

    public static async Task<AssetProfile> GetSummaryBySymbolAsync(string symbol, CancellationToken token = default)
    {
      if (string.IsNullOrWhiteSpace(symbol))
        throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));

      // Initialize necessary components
      await InitAsync(token);

      // Construct the URL
      var url = $"https://query2.finance.yahoo.com/v10/finance/quoteSummary/{Uri.EscapeDataString(symbol)}?modules=assetProfile";

      try
      {
        var response = await url
            .SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson<QuoteSummaryResponse>();

        var industry = response?.QuoteSummary?.Result?[0]?.AssetProfile;

       

        return industry;
      }
      catch (FlurlHttpException ex)
      {
        var errorResponse = await ex.GetResponseStringAsync();
        Console.WriteLine($"Flurl HTTP Error in GetIndustryBySymbolAsync: {ex.Message}\nResponse: {errorResponse}");
        throw new HttpRequestException("Error fetching industry information from Yahoo Finance.", ex);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Unexpected Error in GetIndustryBySymbolAsync: {ex.Message}");
        throw;
      }
    }
    public static async Task<dynamic> GetSummaryRawAsync(string symbol, CancellationToken token = default)
    {
      if (string.IsNullOrWhiteSpace(symbol))
        throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));

      // Initialize necessary components
      await InitAsync(token);

      // Construct the URL
      var url = $"https://query2.finance.yahoo.com/v10/finance/quoteSummary/{Uri.EscapeDataString(symbol)}?modules=assetProfile";

      try
      {
        var response = await url
            .SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson<dynamic>();




        return response;
      }
      catch (FlurlHttpException ex)
      {
        var errorResponse = await ex.GetResponseStringAsync();
        Console.WriteLine($"Flurl HTTP Error in GetIndustryBySymbolAsync: {ex.Message}\nResponse: {errorResponse}");
        throw new HttpRequestException("Error fetching industry information from Yahoo Finance.", ex);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Unexpected Error in GetIndustryBySymbolAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<List<dynamic>> SearchNewsAsync(string query, CancellationToken token = default)
    {
      await InitAsync(token);

      var url = $"https://query1.finance.yahoo.com/v1/finance/search?q={query}";

      try
      {
        var response = await url
            .SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var symbols = response.news;

        if (symbols == null)
        {
          throw new Exception($"No symbols found for query: {query}");
        }

        return symbols;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during SearchSymbolsAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<List<dynamic>> SearchSymbolsAsync(string query, CancellationToken token = default)
    {
      await InitAsync(token);

      var url = $"https://query1.finance.yahoo.com/v1/finance/search?q={query}";

      try
      {
        var response = await url
            .SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var symbols = response.quotes;

        if (symbols == null )
        {
          throw new Exception($"No symbols found for query: {query}");
        }

        return symbols;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during SearchSymbolsAsync: {ex.Message}");
        throw;
      }
    }
    public static async Task<List<dynamic>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate,string interval, CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
      var endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();
      //"chart":{"result":null,"error":{"code":"Bad Request","description":"Invalid input - interval=d is not supported. Valid intervals: [1m, 2m, 5m, 15m, 30m, 60m, 90m, 1h, 1d, 5d, 1wk, 1mo, 3mo]"}}}
      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={endTimestamp}&interval={interval}";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetJsonAsync<dynamic>(token);

        // Extracting quotes and timestamps from the response
        var quotes = response.chart.result[0].indicators.quote[0];
        var timestamps = response.chart.result[0].timestamp;

        // Combining timestamps with quotes into a historical data list
        var historicalData = new List<dynamic>();

        for (int i = 0; i < timestamps?.Count; i++)
        {
          long time = Convert.ToInt64(timestamps[i]);
          historicalData.Add(new
          {
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(time).DateTime,
            Open = quotes.open[i],
            Close = quotes.close[i],
            High = quotes.high[i],
            Low = quotes.low[i],
            Volume = quotes.volume[i]
          });
        }

        return historicalData;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<dynamic> GetCurrentDataAsync(string symbol, CancellationToken token = default)
    {
      await InitAsync(token);

      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1m";

      try
      {
        var response = await GetJsonWithRetryAsync(url, token);

        if (response?.chart?.result?.Length > 0)
        {
          var result = response.chart.result[0];
          var timestamps = result.timestamp;
          var quotes = result.indicators.quote?[0];

          if (timestamps != null && quotes != null && timestamps.Length > 0)
          {
            var mostRecentIndex = timestamps.Length - 1;
            var time = Convert.ToInt64(timestamps[mostRecentIndex]);

            var currentData = new
            {
              Timestamp = DateTimeOffset.FromUnixTimeSeconds(time).DateTime,
              Open = quotes.open[mostRecentIndex],
              Close = quotes.close[mostRecentIndex],
              High = quotes.high[mostRecentIndex],
              Low = quotes.low[mostRecentIndex],
              Volume = quotes.volume[mostRecentIndex]
            };

            return currentData;
          }
          else
          {
            Console.WriteLine("No data available for the provided symbol.");
            return null;
          }
        }
        else
        {
          Console.WriteLine("Unexpected API response format.");
          return null;
        }
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetCurrentDataAsync: {ex.Message}");
        throw;
      }
    }

    private static async Task<dynamic> GetJsonWithRetryAsync(string url, CancellationToken token)
    {
      var retryCount = 0;
      var maxRetryCount = 3;
      var delay = TimeSpan.FromSeconds(2);

      while (retryCount < maxRetryCount)
      {
        try
        {
          return await url
              .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
              .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
              .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
              .GetJsonAsync<dynamic>(token);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response != null && ex.Call.Response.StatusCode == 429)
        {
          // Rate-limiting response; retry after a delay
          retryCount++;
          Console.WriteLine($"Rate limit exceeded. Retrying in {delay.TotalSeconds} seconds...");
          await Task.Delay(delay, token);
          delay = delay * 2; // Exponential backoff
        }
        catch (Exception ex)
        {
          // Rethrow exceptions that are not related to rate limiting
          Console.WriteLine($"Unexpected error during request: {ex.Message}");
          throw;
        }
      }

      throw new Exception("Max retry attempts exceeded.");
    }


    public static async Task<List<dynamic>> GetHistoricalDataAsync(string symbol, DateTime date, CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)date).ToUnixTimeSeconds();

      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={startTimestamp}&interval=1d";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetStringAsync(token);

        var chartResponse = JsonConvert.DeserializeObject<ChartResponse>(response);

        // Combining timestamps with quotes into a historical data list
        var historicalData = new List<dynamic>();
          historicalData.Add(new
          {
            Timestamp = date,
            Open = chartResponse.Chart.Result.FirstOrDefault().Indicators.Quote.FirstOrDefault().Open.FirstOrDefault().Value,
            Close = chartResponse.Chart.Result.FirstOrDefault().Indicators.Quote.FirstOrDefault().Close.FirstOrDefault().Value,
            High = chartResponse.Chart.Result.FirstOrDefault().Indicators.Quote.FirstOrDefault().High.FirstOrDefault().Value,
            Low = chartResponse.Chart.Result.FirstOrDefault().Indicators.Quote.FirstOrDefault().Low.FirstOrDefault().Value,
            Volume = chartResponse.Chart.Result.FirstOrDefault().Indicators.Quote.FirstOrDefault().Volume.FirstOrDefault().Value,
          });

        return historicalData;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
    }
    public static async Task<ChartResponse> GetRealTimeHistoricalJsonDataAsync(string symbol, DateTime period1, DateTime period2,string interval="1m", CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)period1).ToUnixTimeSeconds();
      var endTimestamp = ((DateTimeOffset)period2).ToUnixTimeSeconds();
      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={endTimestamp}&interval={interval}";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetStringAsync(token);

        var chartResponse = JsonConvert.DeserializeObject<ChartResponse>(response);

        return chartResponse;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
    }
    public static async Task<ChartResponse> GetHistoricalJsonDataAsync(string symbol, DateTime period1, DateTime period2, CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)period1).ToUnixTimeSeconds();
      var endTimestamp = ((DateTimeOffset)period2).ToUnixTimeSeconds();
      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={endTimestamp}&interval=1d";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetStringAsync(token);

        var chartResponse = JsonConvert.DeserializeObject<ChartResponse>(response);

        return chartResponse;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<ChartResponse> GetHistoricalJsonDataAsync(string symbol, DateTime date, CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)date).ToUnixTimeSeconds();

      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={startTimestamp}&interval=1d";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetStringAsync(token);

        var chartResponse = JsonConvert.DeserializeObject<ChartResponse>(response);

        return chartResponse;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
    }

    public static async Task<List<dynamic>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
      var endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={endTimestamp}&interval=1d";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetJsonAsync<dynamic>(token);

        // Extracting quotes and timestamps from the response
        var quotes = response.chart.result[0].indicators.quote[0];
        var timestamps = response.chart.result[0].timestamp;

        // Combining timestamps with quotes into a historical data list
        var historicalData = new List<dynamic>();

        for (int i = 0; i < timestamps?.Count; i++)
        {
          long time = Convert.ToInt64(timestamps[i]);
          var Open= quotes.open[i];
          historicalData.Add(new
          {
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(time).DateTime,
            Open = Open,
            Close = quotes.close[i],
            High = quotes.high[i],
            Low = quotes.low[i],
            Volume = quotes.volume[i]
          });
        }

        return historicalData;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
    }
    public static async Task<IEnumerable<dynamic>> GetDividendsAsync(string symbol, DateTime startDate, DateTime endDate,  CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
      var endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();
      var url = $"https://finance.yahoo.com/quote/{symbol}?period1=from&period2=to&interval=div%7Csplit&filter=div&frequency=1d&includeAdjustedClose=true";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetAsync(token);

        // Extracting quotes and timestamps from the response
        var quotes = response.ResponseMessage.Content.ReadAsStringAsync();

        // Combining timestamps with quotes into a historical data list
        var historicalData = new List<dynamic>();
        return historicalData;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
  }

    public static async Task<DetailedQuoteResponse> GetDetailedQuoteAsync(string symbol,CancellationToken token = default)
    {
      // Ensure the auth cookie and crumb have been initialized
      await InitAsync(token);

      // Build the URL with the provided query string parameters.
      var url = $"https://query1.finance.yahoo.com/v7/finance/quote?&symbols={symbol}&fields=currency,fromCurrency,toCurrency,exchangeTimezoneName,exchangeTimezoneShortName,gmtOffSetMilliseconds,regularMarketChange,regularMarketChangePercent,regularMarketPrice,regularMarketTime,preMarketTime,postMarketTime,extendedMarketTime&crumb=00nPqdRgO8P&formatted=false&region=US&lang=en-US";

      try
      {
        // Call the endpoint and deserialize the JSON into our model.
        var response = await url
          .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetJsonAsync<DetailedQuoteResponse>(token);

        // Validate that the response contains the expected data.
        if (response?.quoteResponse?.result == null)
        {
          throw new Exception("Failed to retrieve detailed quote data for AAPL.");
        }

        /* 
         * The expected JSON result will have the following structure:
         * {
         *   "quoteResponse": {
         *     "result": [
         *       {
         *         "language": "en-US",
         *         "region": "US",
         *         "quoteType": "EQUITY",
         *         "typeDisp": "Equity",
         *         "quoteSourceName": "Nasdaq Real Time Price",
         *         "triggerable": true,
         *         "customPriceAlertConfidence": "HIGH",
         *         "regularMarketChangePercent": -0.024286853,
         *         "marketState": "PRE",
         *         "exchange": "NMS",
         *         "exchangeTimezoneName": "America/New_York",
         *         "exchangeTimezoneShortName": "EST",
         *         "gmtOffSetMilliseconds": -18000000,
         *         "market": "us_market",
         *         "esgPopulated": false,
         *         "regularMarketPrice": 247.04,
         *         "currency": "USD",
         *         "preMarketTime": 1740570978,
         *         "postMarketTime": 1740531597,
         *         "regularMarketTime": 1740517200,
         *         "hasPrePostMarketData": true,
         *         "firstTradeDateMilliseconds": 345479400000,
         *         "priceHint": 2,
         *         "regularMarketChange": -0.060012817,
         *         "regularMarketPreviousClose": 247.1,
         *         "fullExchangeName": "NasdaqGS",
         *         "sourceInterval": 15,
         *         "exchangeDataDelayedBy": 0,
         *         "tradeable": false,
         *         "cryptoTradeable": false,
         *         "symbol": "AAPL"
         *       }
         *     ],
         *     "error": null
         *   }
         * }
         */

        return response;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error in GetDetailedQuoteAsync: {ex.Message}");
        throw;
      }
    }


    public static async Task<IEnumerable<dynamic>> GetHistoricalAsync(string symbol, DateTime startDate, DateTime endDate, Period daily, CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
      var endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={endTimestamp}&interval=1d";

      try
      {
        // Sending the request to Yahoo Finance API and receiving the JSON response
        var response = await url
            .SetQueryParam("crumb", Crumb) // Assuming 'Crumb' is a predefined string variable
            .WithCookie(_cookie.Name, _cookie.Value) // Assuming '_cookie' is a predefined object
            .WithHeader(UserAgentKey, UserAgentValue) // Assuming 'UserAgentKey' and 'UserAgentValue' are predefined
            .GetJsonAsync<dynamic>(token);

        // Extracting quotes and timestamps from the response
        var quotes = response.chart.result[0].indicators.quote[0];
        var timestamps = response.chart.result[0].timestamp;

        // Combining timestamps with quotes into a historical data list
        var historicalData = new List<dynamic>();

        for (int i = 0; i < timestamps?.Count; i++)
        {
          long time = Convert.ToInt64(timestamps[i]);
          var Open = quotes.open[i];
          historicalData.Add(new
          {
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(time).DateTime,
            Open = Open,
            Close = quotes.close[i],
            High = quotes.high[i],
            Low = quotes.low[i],
            Volume = quotes.volume[i]
          });
        }

        return historicalData;
      }
      catch (FlurlHttpException ex)
      {
        Console.WriteLine($"Error during GetHistoricalDataAsync: {ex.Message}");
        throw;
      }
    }
  }
}
