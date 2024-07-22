namespace MarketAnalyticHub.Models
{
  public class StockEvent
  {
    public string Date { get; set; }
    public string EventName { get; set; }
    public string Details { get; set; }
    public string Impact { get; set; }
    public string Sentiment { get; set; }
    public string Source { get; set; }

    public decimal? Price { get; set; }
    public decimal? PriceChange { get; set; }

  }
}
