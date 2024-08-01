namespace MarketAnalyticHub.Models
{
  using MarketAnalyticHub.Controllers;
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
    public string Username { get; set; }
    public string Status { get; set; }
    public string Role { get; set; }
    public string TaxId { get; set; }
    public string Contact { get; set; }
    public string Plan { get; set; }
    public DateTime PlanExpiry { get; set; }
    public string PaymentMethod { get; set; }
    public string BillingAddress { get; set; }
    public int TasksDone { get; set; }
    public int ProjectsDone { get; set; }
    public List<string> Languages { get; set; }
    public List<PaymentMethod> PaymentMethods { get; internal set; }
    public List<Address> Addresses { get; internal set; }
  }

}
