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

namespace AspnetCoreMvcFull.Services
{
  public class OpenAIService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
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
      return keywords.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
    }

    public async Task<string> GenerateSentimentImpacts(string description)
    {
      var api = new OpenAIAPI(_apiKey);

      var result = await api.Chat.CreateChatCompletionAsync(new OpenAI_API.Chat.ChatRequest()
      {
        Model = Model.GPT4_Turbo,
        Temperature = 0.1,
        MaxTokens = 50,
        Messages = new OpenAI_API.Chat.ChatMessage[] {
      new OpenAI_API.Chat.ChatMessage(ChatMessageRole.User,  $"{description} Create a sentiment score table for a company's entry into a specific market with a new product. " +
        $"Consider the following factors and their corresponding sentiments based on the provided text:\n\nMarket" +
        $" Potential: Assess the overall growth and demand potential of the target market.\nCompetitive Landscape:" +
        $" Evaluate the level of competition the company will face in the market.\nProduct Innovation: " +
        $"Consider the uniqueness and advantages of the new product compared to existing solutions." +
        $"\nRegulatory Approval: Analyze the potential challenges and risks associated with obtaining necessary regulatory approvals." +
        $"\nFinancial Prospects: Project the potential financial impact and revenue growth for the company if the new product is successful." +
        $"\nBased on these factors, generate the table with the following columns: Factor and Sentiment." +
        $"\n\nTable:\n\nFactor\tSentiment\nMarket Potential" +
        $"\t\nCompetitive Landscape\t\nProduct Innovation\t\nRegulatory Approval\t\nFinancial Prospects" +
        $" format as table"

    )
    }
      });

      var resultText = result.ToString().Trim();
      return resultText;
    }
    public async Task<string> GenerateScoreImpactsJson(string description, string symbol)
    {
      var api = new OpenAIAPI(_apiKey);

      var result = await api.Chat.CreateChatCompletionAsync(new OpenAI_API.Chat.ChatRequest()
      {
        Model = Model.GPT4_Turbo,
        Temperature = 0.1,
        MaxTokens = 500,
        Messages = new OpenAI_API.Chat.ChatMessage[] {
      new OpenAI_API.Chat.ChatMessage(ChatMessageRole.User,  $"impact of the folloing text in stock market " +
      $"{symbol} give a score number betteen 1 and 5 only the number and a why short format as this JSON score:, analysisSummary:  : {description} "
    )
    }
      });

      var resultText = result.ToString().Trim();
      return resultText;
    }
    public async Task<string> GenerateScoreImpacts(string description, string symbol)
    {
      var api = new OpenAIAPI(_apiKey);

      var result = await api.Chat.CreateChatCompletionAsync(new OpenAI_API.Chat.ChatRequest()
      {
        Model = Model.GPT4_Turbo,
        Temperature = 0.1,
        MaxTokens = 500,
        Messages = new OpenAI_API.Chat.ChatMessage[] {
      new OpenAI_API.Chat.ChatMessage(ChatMessageRole.User,  $"impact of the folloing text in stock market " +
      $"{symbol} give a score number betteen 1 and 5 only the number and a why short   : {description} "
    )
    }
      }) ;

      var resultText = result.ToString().Trim();
      return resultText;
    }
    public async Task<string> GenerateIndustryImpacts(string description)
    {
      var api = new OpenAIAPI(_apiKey);

      var result = await api.Chat.CreateChatCompletionAsync(new OpenAI_API.Chat.ChatRequest()
      {
        Model = Model.GPT4_Turbo,
        Temperature = 0.1,
        MaxTokens = 500,
        Messages = new OpenAI_API.Chat.ChatMessage[] {
      new OpenAI_API.Chat.ChatMessage(ChatMessageRole.User,  $"{description} generate sector activity afected by that format as table." +
      $"Score that"


    )
    }
      });

      var resultText = result.ToString().Trim();
      return resultText;
    }
    public async Task<string> SummarizeContent(string content)
    {
      var api = new OpenAIAPI(_apiKey);
      var completionRequest = new CompletionRequest
      {
        Prompt = $"Summarize the following content:\n\n{content}",
        MaxTokens = 150
      };

      var result = await api.Completions.CreateCompletionAsync(completionRequest);
      return result.Completions.FirstOrDefault()?.Text.Trim();
    }
    public async Task<string> GenerateSQLQuery(string naturalLanguageQuery)
    {
      var _api = new OpenAIAPI(_apiKey);
      var completionRequest = new CompletionRequest
      {
        Prompt = $"Translate the following natural language query into SQL: \"{naturalLanguageQuery}\"",
        MaxTokens = 100
      };

      var result = await _api.Completions.CreateCompletionAsync(completionRequest);
      return result.Completions.FirstOrDefault()?.Text.Trim();
    }
    public async Task<string> GetSearchQueryResponse(string query)
    {
      var api = new OpenAIAPI(_apiKey);
      var completionRequest = new CompletionRequest
      {
        Prompt = query,
        MaxTokens = 100
      };

      var result = await api.Completions.CreateCompletionAsync(completionRequest);
      return result.Completions.FirstOrDefault()?.Text.Trim();
    }
  }
}
