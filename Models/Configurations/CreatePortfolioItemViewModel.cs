namespace MarketAnalyticHub.Models.Configurations
{
  public class CreatePortfolioItemViewModel
  {
    public string Symbol { get; set; }
    public string OperationType { get; set; }
    public int PortfolioId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Commission { get; set; }
    public decimal CurrentPrice { get; set; }
  }
}
