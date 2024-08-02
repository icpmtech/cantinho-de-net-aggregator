

using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarketAnalyticHub.Models
{

  public class UserProfileViewModel
  {
    public UserProfile UserProfile { get; set; }
    public List<SelectListItem> AvailableLanguages { get; set; }
    public List<SelectListItem> AvailablePaymentMethods { get; set; }
    public List<SelectListItem> AvailableAddresses { get; set; }
  }
}
