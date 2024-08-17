

namespace MarketAnalyticHub.Controllers
{
  using Lib.Net.Http.WebPush;
  using MarketAnalyticHub.Services;
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;
  using PushSubscription = Services.PushSubscription;

  [ApiController]
  [Route("api/push")]
  public class PushController : ControllerBase
  {
    private readonly PushNotificationService _pushNotificationService;

    public PushController(PushNotificationService pushNotificationService)
    {
      _pushNotificationService = pushNotificationService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendPushNotification([FromBody] PushSubscription subscription)
    {
      // Example dynamic data
      string title = "New Market Insights Available!";
      string message = "Click here to view the latest market trends.";
      string icon = "/assets/icons/marketanalytic_hub_icon_48x48.png";
      string path = "/market-insights";

      await _pushNotificationService.SendPushNotificationAsync(subscription, title, message, icon, path);

      return Ok();
    }
  }

}
