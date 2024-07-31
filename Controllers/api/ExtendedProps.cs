namespace MarketAnalyticHub.Controllers.api
{
  public class ExtendedProps
  {
    public string Impact { get; internal set; }
    public decimal Price { get; internal set; }
    public decimal PriceChange { get; internal set; }
    public int PortfolioItemId { get; internal set; }
  }
}