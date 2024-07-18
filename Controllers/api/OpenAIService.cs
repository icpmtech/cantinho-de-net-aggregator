using Microsoft.Extensions.Configuration;
using OpenAI_API.Completions;
using OpenAI_API;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;

namespace MarketAnalyticHub.Services
{
  public class OpenAIService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient= httpClient;
      _apiKey = configuration["OpenAI:ApiKey"];
    }
    public async Task<string> GetAssociatedCompaniesAsync(string[] keywords)
    {
      var prompt = GeneratePrompt(keywords);

      var api = new OpenAIAPI(_apiKey);
      var completionRequest = new CompletionRequest
      {
        Prompt = prompt,
        MaxTokens = 150,
        Temperature = 0.5,
      };

      var result = await api.Completions.CreateCompletionAsync(completionRequest);

      var finalResponse = result.Completions[0].Text.Trim();
      return finalResponse;
    }

    private string GeneratePrompt(string[] keywords)
    {
      return $"Given the following keywords: {string.Join(", ", keywords)}, list companies that are associated with these terms. Provide the company name, association, sector, and market. in html table";
    }
    public async Task<string[]> GenerateKeywordsAsync(string text, int maxKeywords = 10)
    {
      var api = new OpenAIAPI(_apiKey);
      var completionRequest = new CompletionRequest
      {
        Prompt = $"Generate a list of {maxKeywords} relevant keywords for the following text:\n\n{text}\n\nKeywords:",
        MaxTokens = 50,
        Temperature = 0.5,
      };

      var result = await api.Completions.CreateCompletionAsync(completionRequest);

      var keywords = result.Completions[0].Text.Trim();
      return keywords.Split(new[] { ", " }, System.StringSplitOptions.RemoveEmptyEntries);
    }
  }
}
