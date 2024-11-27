namespace MarketAnalyticHub.Services
{
  using MarketAnalyticHub.Controllers.api;
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
    Task<IEnumerable<Dividend>> GetDividendsAsync(string symbol);
  }


public class YahooFinanceService : IYahooFinanceService
  {

    public async Task<IEnumerable<Dividend>> GetDividendsAsync(string symbol)
    {
      List<Dividend> dividends = new List<Dividend>();
      try
      {
        var _dividends = await Yahoo.GetDividendsAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1));

        foreach (var candle in _dividends)
        {
          dividends.Append(new Dividend { Amount = candle.Dividend, ExDate = candle.DateTime });
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
  }

}
