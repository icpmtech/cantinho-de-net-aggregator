namespace MarketAnalyticHub.Services
{
  using DocumentFormat.OpenXml.Spreadsheet;
  using Lib.Net.Http.WebPush;
  using Lib.Net.Http.WebPush.Authentication;
  using Microsoft.Extensions.Configuration;
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using System.Threading.Tasks;

  public class PushNotificationService
  {
    private readonly PushServiceClient _pushClient;
    private readonly VapidAuthentication _vapidAuth;

    public PushNotificationService(IConfiguration configuration)
    {
      _pushClient = new PushServiceClient();
      var publicKey = configuration["VapidKeys:PublicKey"];
      var privateKey = configuration["VapidKeys:PrivateKey"];

      if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
      {
        throw new ArgumentException("VAPID keys are missing or invalid.");
      }

      _pushClient = new PushServiceClient();
      _vapidAuth = new VapidAuthentication(publicKey, privateKey);

      _pushClient.DefaultAuthentication = _vapidAuth;
    }

    public async Task SendPushNotificationAsync(PushSubscription subscription, string payloadJson)
    {
      

      var notification = new PushMessage(payloadJson); // Pass the JSON payload as the message
      notification.Topic = Guid.NewGuid().ToString(); // Unique identifier for the push message
      notification.Urgency = PushMessageUrgency.Normal;

      try
      {
        await _pushClient.RequestPushMessageDeliveryAsync(subscription.ToLibSubscription(), notification);
      }
      catch (Exception ex)
      {
        // Handle any exceptions
        Console.WriteLine($"Error sending push notification: {ex.Message}");
      }
    }
  }


    public class PushSubscription
  {
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }

    [JsonPropertyName("keys")]
    public PushSubscriptionKeys Keys { get; set; }

    // Method to convert to Lib.Net.Http.WebPush.PushSubscription
    public Lib.Net.Http.WebPush.PushSubscription ToLibSubscription()
    {
      return new Lib.Net.Http.WebPush.PushSubscription
      {
        Endpoint = this.Endpoint,
        Keys = new Dictionary<string, string>
                {
                    { "p256dh", this.Keys.P256DH },
                    { "auth", this.Keys.Auth }
                }
      };
    }
  }

  public class PushSubscriptionKeys
  {
    [JsonPropertyName("p256dh")]
    public string P256DH { get; set; }

    [JsonPropertyName("auth")]
    public string Auth { get; set; }
  }


}
