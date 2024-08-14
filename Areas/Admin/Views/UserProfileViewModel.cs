using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarketAnalyticHub.Areas.Admin.Views
{

  public class UserProfileViewModel
  {
    public UserProfile UserProfile { get; set; }
    public List<SelectListItem> AvailableLanguages { get; set; }
    public List<SelectListItem> AvailablePaymentMethods { get; set; }
    public List<SelectListItem> AvailableAddresses { get; set; }
  }
}
