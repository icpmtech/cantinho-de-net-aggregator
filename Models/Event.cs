namespace MarketAnalyticHub.Models
{
  public class Event
  {
    public int Id { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public bool? AllDay { get; set; }
    public ExtendedProps? ExtendedProps { get; internal set; }
  }
  public class ExtendedProps
  {
    public string Calendar { get; set; }
    public string Location { get; set; }
    public string Guests { get; set; }
    public string Description { get; set; }
    public string Impact { get; set; }
    public string Sentiment { get; set; }
    public string Source { get; set; }
    public decimal? Price { get; set; }
    public decimal? PriceChange { get; set; }
    public int PortfolioItemId { get; set; }
  }
}
