using Microsoft.Extensions.Configuration;
using OpenAI_API.Completions;
using OpenAI_API;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Services
{
  public class OpenAIService
  {
    private readonly string _apiKey;

    public OpenAIService(IConfiguration configuration)
    {
      _apiKey = configuration["OpenAI:ApiKey"];
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
