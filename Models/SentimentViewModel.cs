using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Models
{
  public class SentimentViewModel
  {
    public IEnumerable<MarketAnalyticHub.Models.Portfolio.Portfolio> Portfolios { get; set; }
    public List<StockEvent> PositiveEvents { get; set; }
    public List<StockEvent> NegativeEvents { get; set; }
  }

}
