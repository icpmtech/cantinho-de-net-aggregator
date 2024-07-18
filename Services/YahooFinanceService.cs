namespace AspnetCoreMvcFull.Services
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using YahooFinanceApi;

  public interface IYahooFinanceService
  {
    Task<StockData> GetRealTimePriceAsync(string symbol);
    Task<List<HistoricalData>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate);

  }



public class YahooFinanceService : IYahooFinanceService
{
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

    public async Task<List<HistoricalData>> GetHistoricalDataAsync(string symbol, DateTime startDate, DateTime endDate)
    {
        try
        {
            var historicalData = await Yahoo.GetHistoricalAsync(symbol, startDate, endDate);
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
}

}
