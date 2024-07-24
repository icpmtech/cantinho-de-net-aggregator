

using System.ComponentModel.DataAnnotations;

namespace MarketAnalyticHub.Models
{
  public class ProfileAvatar
  {
    [Required]
    public IFormFile Avatar { get; set; }
  }
}
