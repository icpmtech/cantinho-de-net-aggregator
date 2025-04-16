using MarketAnalyticHub.Models.Portfolio.ViewModels;

public class ViewModelPortfolioDividend
{
  public int Id { get; set; }
  public string Symbol { get; set; }
  public int PortfolioItemId { get; set; }
  public decimal Amount { get; set; }
  public DateTime? ExDate { get; set; }
  public DateTime? PaymentDate { get; set; }
  public int Quantity { get; set; }
  public DateTime? PurchaseDate { get; set; }
}
