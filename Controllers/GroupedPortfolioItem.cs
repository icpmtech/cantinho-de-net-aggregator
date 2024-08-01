using MarketAnalyticHub.Models.Portfolio;

namespace MarketAnalyticHub.Controllers
{
  internal class GroupedPortfolioItem
  {
    public string Symbol { get; set; }
    public List<PortfolioItem> Items { get; set; }
  }
}
