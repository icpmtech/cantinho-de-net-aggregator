namespace MarketAnalyticHub.Controllers.AIPilot
{
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.SignalR;
  using Microsoft.Extensions.Configuration;
  using System.Net.Http;
  using System.Text;
  using System.Text.Json;
  using System.Threading.Tasks;

 
  public class ChatHub : Hub
  {
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ChatHub(IConfiguration configuration, HttpClient httpClient)
    {
      _configuration = configuration;
      _httpClient = httpClient;
    }

    public async Task SendMessage(string user, string message)
    {
      var aiResponse = await GetOpenAIResponse(message);
      await Clients.All.SendAsync("ReceiveMessage", user, message);
      await Clients.All.SendAsync("ReceiveMessage", "AIPilot", aiResponse);
    }

    private async Task<string> GetOpenAIResponse(string userMessage)
    {
      var apiKey = _configuration["OpenAI:ApiKey"];
      var request = new
      {
        model = "gpt-3.5-turbo",
        messages = new[]
          {
            new { role = "user", content = userMessage }
        },
        temperature = 0.7
      };

      var requestBody = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
      _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

      var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestBody);
      var responseContent = await response.Content.ReadAsStringAsync();
      var jsonResponse = JsonDocument.Parse(responseContent);
      return jsonResponse.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString().Trim();
    }

  }

}
