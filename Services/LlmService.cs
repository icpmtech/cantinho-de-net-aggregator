using Microsoft.Extensions.Configuration;
using OpenAI_API.Completions;
using OpenAI_API;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AspnetCoreMvcFull.Services
{
  public class LlmService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public LlmService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _apiKey = configuration["OpenAI:ApiKey"];
    }
    public async Task<string> GeneratePortfolioReportAsync(string prompt)
    {
      var requestBody = new
      {
        model = "gpt-4",
        prompt = prompt,
        max_tokens = 150,
        temperature = 0.7
      };
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
      var content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("https://api.openai.com/v1/completions", content);

      if (response.IsSuccessStatusCode)
      {
        var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
        return result.choices[0].text;
      }

      return "Error generating report.";
    }

  }
}
