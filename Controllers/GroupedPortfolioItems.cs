using MarketAnalyticHub.Models.Portfolio.Entities;

namespace MarketAnalyticHub.Models
{
  public class GroupedPortfolioItems
  {
    public string Symbol { get; set; }
    public List<PortfolioItem> Items { get; set; }
  }
}
