using MarketAnalyticHub.Models.Portfolio.Entities;

public class Dividend
{
  public int Id { get; set; }
  public int PortfolioItemId { get; set; }
  public PortfolioItem? PortfolioItem { get; set; }
  public string Symbol { get; set; }
  public decimal Amount { get; set; }
  public DateTime? ExDate { get; set; }
  public DateTime? PaymentDate { get; set; }
}
