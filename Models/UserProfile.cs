namespace MarketAnalyticHub.Models
{
  using System.ComponentModel.DataAnnotations;

  public class UserProfile
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string Organization { get; set; }

    [Phone]
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
