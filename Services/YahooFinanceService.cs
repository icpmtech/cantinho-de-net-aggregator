namespace MarketAnalyticHub.Services
{
  using MarketAnalyticHub.Controllers.api;
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Services.ApiDataApp.Services;
  using MarketAnalyticHub.YaooServive.Models;
  using Microsoft.Graph;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using YahooFinanceApi;

  public interface IYahooFinanceService
  {
    Task<StockData> GetRealTimePriceAsync(string symbol);
    Task<List<HistoricalData>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate, Period period);
    Task<List<HistoricalData>> GetDailyHistoricalDataAsync(string symbol, DateTime startDate, DateTime now);
    Task<List<HistoricalData>> GetDailyHistoricalDataAsync(string symbol, DateTime dateTime);
    Task<IEnumerable<DividendScreenViewModel>> GetDividendsAsync(string symbol, DateTime startDate, DateTime endDate);
    Task<StockViewModel?> GetStockDataAsync(string symbol);
    Task<List<ChartDataPoint>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate);
    Task<List<NewsItemScreener>> GetMockNews(string stockSymbol);
    Task<StockViewModel?> GetSummaryBySymbolAsync(string stockSymbol);
    Task<StockFinantialsViewModel?> GetFinancialsBySymbolAsync(string stockSymbol);
  }


public  class YahooFinanceService : IYahooFinanceService
  {
   

    public async Task<StockViewModel?> GetStockDataAsync(string symbol)
    {
      try
      {
        // Fetch real-time stock data
        var securities = await Yahoo.Symbols(symbol).Fields(
            Field.Symbol,
            Field.RegularMarketPrice,
            Field.ShortName,
            Field.MarketCap,
            Field.TrailingPE,
            Field.EpsTrailingTwelveMonths,
            Field.RegularMarketDayHigh,
            Field.RegularMarketDayLow,
            Field.FiftyTwoWeekHigh,
            Field.FiftyTwoWeekLow,
            Field.RegularMarketVolume,
            Field.RegularMarketChange,
             Field.Currency,
             Field.DividendDate,
             Field.TrailingAnnualDividendRate,
             Field.BookValue,
             Field.Exchange,
             Field.TrailingAnnualDividendYield,
             Field.FiftyTwoWeekHighChange,
             Field.FiftyTwoWeekHighChangePercent,
             Field.FiftyTwoWeekLowChange,
             Field.FiftyTwoWeekLowChangePercent,
               Field.PostMarketChange,
     Field.PostMarketChangePercent,
     Field.PostMarketPrice,
     Field.PostMarketTime,
     Field.MarketState


        ).QueryAsync();

        if (!securities.ContainsKey(symbol))
        {
          return null; // Symbol not found
        }

        var security = securities[symbol];
        var stockViewModel = new StockViewModel
        {
          Symbol = Utility.GetValidString(symbol, "N/A"),
          CompanyName = Utility.GetValidString(SafeValueUtil.GetValueOrDefault(() => security?.ShortName, "Unknown"), "Unknown"),
          Price = SafeValueUtil.GetValueOrDefault(() => security?.RegularMarketPrice ?? 0.0, 0.0),
          MarketCap = SafeValueUtil.GetValueOrDefault(() => security?.MarketCap ?? 0, 0),
          PERatio = SafeValueUtil.GetValueOrDefault(() => security?.TrailingPE ?? 0.0, 0.0),
          EPS = SafeValueUtil.GetValueOrDefault(() => security?.EpsTrailingTwelveMonths ?? 0, 0),
          FiftyTwoWeekHigh = SafeValueUtil.GetValueOrDefault(() => security?.FiftyTwoWeekHigh ?? 0, 0),
          FiftyTwoWeekLow = SafeValueUtil.GetValueOrDefault(() => security?.FiftyTwoWeekLow ?? 0, 0),
          Volume = SafeValueUtil.GetValueOrDefault(() => security?.RegularMarketVolume ?? 0L, 0L),
          Change = SafeValueUtil.GetValueOrDefault(() => security?.RegularMarketChange ?? 0, 0),
          Currency= Utility.GetValidString(SafeValueUtil.GetValueOrDefault(() => security?.Currency, "Unknown"), "Unknown"),
          DividendDate = SafeValueUtil.GetValueOrDefault(() => security?.DividendDate , 0), // Substituir conforme necessário
          BookValue = SafeValueUtil.GetValueOrDefault(() => security?.BookValue , 0), // Substituir conforme necessário
          Exchange = Utility.GetValidString(security?.Exchange ?? "Unknown", "Unknown"), // Substituir conforme necessário
          FiftyTwoWeekHighChange = SafeValueUtil.GetValueOrDefault(() => security?.FiftyTwoWeekHighChange, 0),
          FiftyTwoWeekHighChangePercent = SafeValueUtil.GetValueOrDefault(() => security?.FiftyTwoWeekHighChangePercent , 0.0),
          FiftyTwoWeekLowChange = SafeValueUtil.GetValueOrDefault(() => security?.FiftyTwoWeekLowChange,0),
          FiftyTwoWeekLowChangePercent = SafeValueUtil.GetValueOrDefault(() => security?.FiftyTwoWeekLowChangePercent , 0.0),
          TrailingAnnualDividendYield= SafeValueUtil.GetValueOrDefault(() => security?.TrailingAnnualDividendYield, 0.0),
          PostMarketChange = SafeValueUtil.GetValueOrDefault(() => security?.PostMarketChange, 0.0),
          PostMarketChangePercent = SafeValueUtil.GetValueOrDefault(() => security?.PostMarketChangePercent, 0.0),
          PostMarketPrice = SafeValueUtil.GetValueOrDefault(() => security?.PostMarketPrice, 0.0),
          PostMarketTime = SafeValueUtil.GetValueOrDefault(() => security?.PostMarketTime, 0),
          MarketState= Utility.GetValidString(SafeValueUtil.GetValueOrDefault(() => security?.MarketState, "Unknown"), "Unknown"),

        };
        return stockViewModel;

      }
      catch (Exception ex)
      {
        // Log the exception and return null
        Console.WriteLine($"Error fetching data for {symbol}: {ex.Message}");
        return null;
      }
    }
    public async Task<StockFinantialsViewModel?> GetFinancialsBySymbolAsync(string symbol)
    {
      try
      {
        var client = new HttpClient();
        var apiKey = "60c4b7c12emshfe21a6a1ec58d8bp1a396cjsn6bc88b0f02f5"; // Securely retrieve your API key
        var request = new HttpRequestMessage
        {
          Method = HttpMethod.Get,
          RequestUri = new Uri($"https://yahoo-finance166.p.rapidapi.com/api/stock/get-financial-data?region=US&symbol={symbol}"),
          Headers =
            {
                { "x-rapidapi-key", apiKey },
                { "x-rapidapi-host", "yahoo-finance166.p.rapidapi.com" },
            },
        };

        using (var response = await client.SendAsync(request))
        {
          response.EnsureSuccessStatusCode();
          var body = await response.Content.ReadAsStringAsync();

          // Deserialize the JSON response
          var root = JsonConvert.DeserializeObject<dynamic>(body);

          if (root?.quoteSummary?.result == null || root?.quoteSummary.result.Count == 0)
          {
            Console.WriteLine("Financial data not found.");
            return null;
          }

          var financialData = root?.quoteSummary.result[0].financialData;

          var stockViewModel = new StockFinantialsViewModel
          {
            Symbol = symbol.ToUpper(),
            CurrentPrice = financialData.currentPrice?.raw,
            TargetHighPrice = financialData.targetHighPrice?.raw,
            TargetLowPrice = financialData.targetLowPrice?.raw,
            TargetMeanPrice = financialData.targetMeanPrice?.raw,
            TargetMedianPrice = financialData.targetMedianPrice?.raw,
            RecommendationMean = financialData.recommendationMean?.raw,
            RecommendationKey = financialData.recommendationKey,
            NumberOfAnalystOpinions = (int?)financialData.numberOfAnalystOpinions?.raw,
            TotalCash = financialData.totalCash?.raw,
            TotalCashPerShare = financialData.totalCashPerShare?.raw,
            Ebitda = financialData.ebitda?.raw,
            TotalDebt = financialData.totalDebt?.raw,
            QuickRatio = financialData.quickRatio?.raw,
            CurrentRatio = financialData.currentRatio?.raw,
            TotalRevenue = financialData.totalRevenue?.raw,
            DebtToEquity = financialData.debtToEquity?.raw,
            RevenuePerShare = financialData.revenuePerShare?.raw,
            ReturnOnAssets = financialData.returnOnAssets?.raw,
            ReturnOnEquity = financialData.returnOnEquity?.raw,
            FreeCashflow = financialData.freeCashflow?.raw,
            OperatingCashflow = financialData.operatingCashflow?.raw,
            EarningsGrowth = financialData.earningsGrowth?.raw,
            RevenueGrowth = financialData.revenueGrowth?.raw,
            GrossMargins = financialData.grossMargins?.raw,
            EbitdaMargins = financialData.ebitdaMargins?.raw,
            OperatingMargins = financialData.operatingMargins?.raw,
            ProfitMargins = financialData.profitMargins?.raw,
            FinancialCurrency = financialData.financialCurrency,
            

          };

          return stockViewModel;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error fetching data for {symbol}: {ex.Message}");
        return null;
      }
    }




    public async Task<List<ChartDataPoint>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate)
    {
      try
      {
        var historicalData = await YahooService.GetHistoricalAsync(symbol, startDate, endDate, Period.Daily);

        return historicalData.Select(h => new ChartDataPoint
        {
          Date = h.Timestamp,
          Close = Convert.ToDecimal(h.Close)
        }).ToList();
      }
      catch (Exception ex)
      {
        // Log the exception and return an empty list
        Console.WriteLine($"Error fetching historical data for {symbol}: {ex.Message}");
        return new List<ChartDataPoint>();
      }
    }
    public async Task<IEnumerable<DividendScreenViewModel>> GetDividendsAsync(string symbol, DateTime startDate, DateTime endDate)
    {
      List<DividendScreenViewModel> dividends = new List<DividendScreenViewModel>();
      try
      {
        var _dividends = await YahooService.GetDividendsAsync(symbol, startDate, endDate);

        foreach (var candle in _dividends)
        {
          dividends.Append(new DividendScreenViewModel {  Amount = candle.Dividend.ToString(), ExDate = candle.DateTime.ToString() });
        }



        return dividends;
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        //_logger.LogError(ex, $"Failed to get current price for symbol: {symbol}");
        return dividends;
      }
    }

    public async Task<StockData> GetRealTimePriceAsync(string symbol)
    {
      try
      {
        var securities = await Yahoo.Symbols(symbol)
            .Fields(Field.Symbol, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent, Field.RegularMarketDayHigh, Field.RegularMarketDayLow, Field.RegularMarketOpen, Field.RegularMarketPreviousClose)
            .QueryAsync();

        var security = securities[symbol];

        var stockData = new StockData
        {
          CurrentPrice = security[Field.RegularMarketPrice],
          Change = security[Field.RegularMarketChange],
          PercentChange = security[Field.RegularMarketChangePercent],
          HighPrice = security[Field.RegularMarketDayHigh],
          LowPrice = security[Field.RegularMarketDayLow],
          OpenPrice = security[Field.RegularMarketOpen],
          PreviousClosePrice = security[Field.RegularMarketPreviousClose]
        };

        return stockData;
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        //_logger.LogError(ex, $"Failed to get current price for symbol: {symbol}");
        return new StockData
        {
          CurrentPrice = 0,
          Change = 0,
          PercentChange = 0,
          HighPrice = 0,
          LowPrice = 0,
          OpenPrice = 0,
          PreviousClosePrice = 0
        };
      }
    }

    public async Task<List<HistoricalData>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate, Period period )
    {
      try
      {
        var historicalData = await Yahoo.GetHistoricalAsync(symbol, startDate, endDate, period);
        return historicalData.Select(data => new HistoricalData
        {
          Date = data.DateTime,
          Open = data.Open,
          High = data.High,
          Low = data.Low,
          Close = data.Close,
          Volume = data.Volume
        }).ToList();
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        //_logger.LogError(ex, $"Failed to get historical data for symbol: {symbol}");
        return new List<HistoricalData>();
      }
    }

    public async Task<List<HistoricalData>> GetDailyHistoricalDataAsync(string symbol, DateTime startDate, DateTime now)
    {
      try
      {
        var data = await Yahoo.GetHistoricalAsync(symbol, startDate, now, Period.Daily);
        return data.Select(data => new HistoricalData
        {
          Date = data.DateTime,
          Open = data.Open,
          High = data.High,
          Low = data.Low,
          Close = data.Close,
          Volume = data.Volume
        }).ToList();
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        //_logger.LogError(ex, $"Failed to get historical data for symbol: {symbol}");
        return new List<HistoricalData>();
      }
    }

    public async Task<List<HistoricalData>> GetDailyHistoricalDataAsync(string symbol, DateTime dateTime)
    {
      try
      {
        var data = await YahooService.GetHistoricalDataAsync(symbol, dateTime);
        return data?.Select(data => new HistoricalData
        {
          Date = dateTime,
          Open = data.Open,
          High = data.High,
          Low = data.Low,
          Close = data.Close,
          Volume = data.Volume
        }).ToList();
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        //_logger.LogError(ex, $"Failed to get historical data for symbol: {symbol}");
        return new List<HistoricalData>();
      }
    }
    public static class TimeUtil
    {
      public static DateTime ConvertLongToDateTime(long timestamp, bool isMilliseconds = true)
      {
        return isMilliseconds
            ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime
            : DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
      }
    }

    public async Task<List<NewsItemScreener>> GetMockNews(string stockSymbol)
    {
      try
      {
        var data = await YahooService.SearchNewsAsync(stockSymbol);
        return data?.Select(data =>
        new NewsItemScreener
        {
          Title = data.title,
          Url = data.link,
          PublishedDate = TimeUtil.ConvertLongToDateTime(data.providerPublishTime),
          Source = data.link
        }).ToList();
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        //_logger.LogError(ex, $"Failed to get historical data for symbol: {symbol}");
        return new List<NewsItemScreener>();
      }
    }

    public async Task<StockViewModel?> GetSummaryBySymbolAsync(string stockSymbol)
    {
      try
      {
        var data = await YahooService.GetSummaryBySymbolAsync(stockSymbol);
        CompanyOfficer ceo = data.CompanyOfficers.FirstOrDefault(o => o.Title.Contains("CEO"));

      
          return 
        new StockViewModel
        {
          CEO= ceo?.Name ?? "N/A",
          Sector = data.Sector,
          Industry = data.Industry,
          Description=data.LongBusinessSummary
        };
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        //_logger.LogError(ex, $"Failed to get historical data for symbol: {symbol}");
        return new StockViewModel();
      }
    }
  }

}
