

namespace MarketAnalyticHub.Controllers
{
  using Lib.Net.Http.WebPush;
  using MarketAnalyticHub.Services;
  using Microsoft.AspNetCore.Mvc;
  using System.Text.Json;
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
      var notificationPayload = new
      {
        title = "New Market Insights Available!",
        body = "Click here to view the latest market trends.",
        icon = "/assets/icons/marketanalytic_hub_icon_48x48.png",
        path = "/"
      };

      string payloadJson = JsonSerializer.Serialize(notificationPayload);

      await _pushNotificationService.SendPushNotificationAsync(subscription, payloadJson);

      return Ok();
    }

  }

}
