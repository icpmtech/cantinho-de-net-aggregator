namespace MarketAnalyticHub.Controllers.AIPilot
{
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.SignalR;
  using Microsoft.Extensions.Configuration;
  using System.Net.Http;
  using System.Text;
  using System.Text.Json;
  using System.Threading.Tasks;


  using Microsoft.AspNetCore.SignalR;
  using System.Threading.Tasks;

  public class NotificationHub : Hub
  {
    // Method to send real-time portfolio updates to a specific user
    public async Task SendPortfolioUpdate(string userId, string message)
    {
      await Clients.User(userId).SendAsync("ReceivePortfolioUpdate", message);
    }
  }

}
