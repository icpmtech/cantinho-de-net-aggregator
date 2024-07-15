namespace MarketAnalyticHub.Models
{
  using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore;

  public class AppIdentityDbContext : IdentityDbContext<ApplicationUser>
  {
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : base(options)
    {
    }
  }

}
