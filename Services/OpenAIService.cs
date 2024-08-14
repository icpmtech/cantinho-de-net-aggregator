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
using OpenAI_API.Embedding;
using Google.Protobuf.WellKnownTypes;
using Nest;

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

    public async Task<string> GenerateEnhancedQueryAsync(string query)
    {
      // Call OpenAI API to enhance the query
      // Example: Use GPT-3/4 to refine or add context to the query
      return await CallOpenAiApiAsync(query);
    }

    public async Task<string> AnswerQuestionAsync(string question)
    {
      // Call OpenAI API to get a direct answer
      return await CallOpenAiApiAsync(question);
    }

    public async Task<List<string>> GenerateSummariesAsync(IEnumerable<dynamic> documents)
    {
      // Summarize the content of the documents
      var summaries = new List<string>();
      foreach (var doc in documents)
      {
        var summary = await CallOpenAiApiAsync(doc.ToString());
        summaries.Add(summary);
      }
      return summaries;
    }
    public async Task<string> GetChatResponseAsync(string query)
    {
      var api = new OpenAIAPI(_apiKey);

      var result = await api.Chat.CreateChatCompletionAsync(new OpenAI_API.Chat.ChatRequest()
      {
        Model = Model.GPT4_Turbo,
        Temperature = 0.1,
        MaxTokens = 300,
        Messages = new OpenAI_API.Chat.ChatMessage[] {
      new OpenAI_API.Chat.ChatMessage(ChatMessageRole.User,  $"{query}"

    )
    }
      });

      var resultText = result.ToString().Trim();
      return resultText;
    }
    public async Task<bool> IsQuestionAsync(string query)
    {
      // Determine if the query is a question
      var result = await CallOpenAiApiAsync($"Is this a question? {query}");
      return result.ToLower().Contains("yes");
    }


    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
      // Create an embedding request
      var embeddingRequest = new EmbeddingRequest
      {
        Input = text,
        Model = "text-embedding-ada-002" // Specify the model for embeddings
      };
      var api = new OpenAI_API.OpenAIAPI(_apiKey);
      // Call the API to get the embedding result
      var embeddingResult = await api.Embeddings.CreateEmbeddingAsync(embeddingRequest);

      if (embeddingResult == null || embeddingResult.Data == null || embeddingResult.Data.Count == 0)
      {
        throw new Exception("Failed to generate embedding or no data returned.");
      }

      // Convert the embedding to a float array and return
      return embeddingResult.Data.First().Embedding;
    }

   

    private async Task<string> CallOpenAiApiAsync(string input)
    {
      // Initialize the OpenAI API client with the provided API key
      var api = new OpenAIAPI(_apiKey);

      // Create a completion request with the desired parameters
      var completionRequest = new CompletionRequest()
      {
        Prompt = input,
        MaxTokens = 50, // Adjust max tokens based on the expected length of the response
        Temperature = 0.5, // Adjust temperature based on the desired creativity of the response
      };

      // Call the API to get the completion result
      var result = await api.Completions.CreateCompletionAsync(completionRequest);

      // Extract and return the generated text from the response
      return result.Completions[0].Text.Trim();
    }

    private float[] ConvertToFloatArray(string embedding)
    {
      // Convert the string embedding to a float array
      return new float[] { 0.0f }; // Example
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

    public async Task<string> AnalyzeSentimentAsync(string content)
    {
      var api = new OpenAIAPI(_apiKey);
      // Create a prompt for sentiment analysis
      var prompt = $"Analyze the sentiment of the following text: \"{content}\". Respond with 'Positive', 'Negative', or 'Neutral'.";

      var completionRequest = new CompletionRequest
      {
        Prompt = prompt,
        MaxTokens = 100
      };

      var result = await api.Completions.CreateCompletionAsync(completionRequest);
      return result.Completions.FirstOrDefault()?.Text.Trim();
    }
  }
}
