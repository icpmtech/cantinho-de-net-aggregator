using System.ComponentModel.DataAnnotations;

namespace MarketAnalyticHub.Models
{
  public class PortfolioLossRule
  {
    [Key]
    public int Id { get; set; }
    public decimal LossThreshold { get; set; }
    // Add other properties as needed, e.g., RuleName, Description, etc.
  }



}
