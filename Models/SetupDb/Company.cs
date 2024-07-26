using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Models.SetupDb
{
  public class Company
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Industry { get; set; } = "NA";

    public SymbolItem? Symbol { get; set; }
    public List<Sector>? Sectors { get; set; }
    public List<QualitativeEvent>? QualitativeEvents { get; set; }
    public List<News.NewsItem>? News { get; set; }
  }
}
