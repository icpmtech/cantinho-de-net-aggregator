namespace MarketAnalyticHub.Models
{
  public class Asset
  {
    public int Id { get; set; }
    public string Ticker { get; set; }
    public string Name { get; set; }
    public string Sector { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }
    public virtual ICollection<AssetPerformance> Performances { get; set; }
  }

  public class AssetPerformance
  {
    public int Id { get; set; }
    public int AssetId { get; set; }
    public virtual Asset Asset { get; set; }
    public string Period { get; set; } // "today", "1week", "1month", "1year"
    public decimal PerformancePercent { get; set; }
    public DateTime Date { get; set; }
  }

  // DTOs
  public class AssetDto
  {
    public int Id { get; set; }
    public string Ticker { get; set; }
    public string Name { get; set; }
    public string Sector { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Dictionary<string, decimal> Variations { get; set; } = new Dictionary<string, decimal>();
    public decimal TotalValue => Price * Quantity;
  }

  public class PortfolioSummaryDto
  {
    public decimal TotalValue { get; set; }
    public decimal Performance { get; set; }
    public string Period { get; set; }
    public string TopPerformerTicker { get; set; }
    public decimal TopPerformerValue { get; set; }
    public string WorstPerformerTicker { get; set; }
    public decimal WorstPerformerValue { get; set; }
  }
}
