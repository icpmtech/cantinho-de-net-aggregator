using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers.api
{
  internal static class YahooService
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

    public static async Task<List<dynamic>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken token = default)
    {
      await InitAsync(token);

      var startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
      var endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

      var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTimestamp}&period2={endTimestamp}&interval=1d";
      
      try
      {
        var response = await url.SetQueryParam("crumb", Crumb)
            .WithCookie(_cookie.Name, _cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync(token)
            .ReceiveJson();

        var quotes = response.chart.result[0].indicators.quote[0];
        var timestamps = response.chart.result[0].timestamp;

        var historicalData = timestamps.ToList();
        
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
