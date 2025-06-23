namespace MarketAnalyticHub.Areas.Admin.Models
{
  internal class DeleteUserViewModel
  {
    public string UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public IList<string> Roles { get; set; }
  }
}
