using MarketAnalyticHub.Models.Portfolio.Entities;

namespace MarketAnalyticHub.Controllers
{
  public class GroupedPortfolioItem
  {
    public string Symbol { get; set; }
    public List<PortfolioItem> Items { get; set; }
  }
}
