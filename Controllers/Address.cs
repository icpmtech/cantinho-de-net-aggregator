namespace MarketAnalyticHub.Models
{
  public class Address
  {
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Country { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string Landmark { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public bool IsBillingAddress { get; set; }
    public int UserProfileId { get; set; }
    public virtual UserProfile UserProfile { get; set; }
  }
}
