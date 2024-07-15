namespace MarketAnalyticHub.Models
{
  using Microsoft.AspNetCore.Identity;

  public class ApplicationUser : IdentityUser
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Organization { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string Language { get; set; }
    public string TimeZone { get; set; }
    public string Currency { get; set; }
    public string AvatarUrl { get; set; }
  }

}
