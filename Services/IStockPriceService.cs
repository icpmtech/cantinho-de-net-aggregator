using MarketAnalyticHub.Controllers.api;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Services
{
  using System;
  using System.Globalization;
  using System.Net.Http;
  using System.Threading.Tasks;

  public interface IStockPriceService
  {
    Task<List<HistoricalData>> GetHistoricalPriceAsync(string symbol, DateTime date);
  }
  public class StockPriceService : IStockPriceService
  {
    private readonly IYahooFinanceService _yahooFinanceService;
    public StockPriceService(IYahooFinanceService yahooFinanceService)
    {
      _yahooFinanceService = yahooFinanceService;
    }



    public async Task<List<HistoricalData>> GetHistoricalPriceAsync(string symbol, DateTime date)
    {
     return  await _yahooFinanceService.GetDailyHistoricalDataAsync(symbol, date);
    }
  }




}
