namespace MarketAnalyticHub.Models
{
  public class UserCredit
  {
    public int Id { get; set; }
    public int UserId { get; set; } // Foreign key to UserProfile
    public int TotalCredits { get; set; }
    public int UsedCredits { get; set; }

    // This property is calculated and doesn't need to be stored in the database
    public int RemainingCredits => TotalCredits - UsedCredits;

    // Navigation property
    public UserProfile UserProfile { get; set; }
  }

}
