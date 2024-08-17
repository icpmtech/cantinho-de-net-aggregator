using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketAnalyticHub.Models
{
  public class PortfolioLossRule
  {
    [Key]
    public int Id { get; set; }

    public decimal LossThreshold { get; set; }

    // Foreign key to UserProfile
    [Required]
    public int UserProfileId { get; set; }

    // Navigation property to UserProfile
    [ForeignKey("UserProfileId")]
    public UserProfile UserProfile { get; set; }
  }

  public class PortfolioLossRuleViewModel
  {

    public int Id { get; set; }
    public decimal LossThreshold { get; set; }

    
  }

}
