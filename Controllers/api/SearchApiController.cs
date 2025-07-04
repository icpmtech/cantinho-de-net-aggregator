using AspnetCoreMvcFull.Services;
using Elasticsearch.Net;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services.Elastic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nest;


[Route("api/[controller]")]
[ApiController]
public class SearchController : ControllerBase
{
  private readonly ElasticClient _elasticClient;
  private readonly OpenAIService _openAiClient;
  private readonly UserManager<ApplicationUser> _userManager;
  public SearchController(ElasticSearchService elasticClient, OpenAIService openAiClient, UserManager<ApplicationUser> userManager)
  {
    _elasticClient = elasticClient._client;
    _openAiClient = openAiClient;
    _userManager = userManager;
  }

  [HttpGet("search")]
  public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 10)
  {
    // Garantir que a página seja pelo menos 1 para evitar problemas com páginas negativas ou zero
    if (page < 1) page = 1;

    // Calcular o ponto inicial para a paginação
    var from = (page - 1) * pageSize;

    // Definir os campos para buscar e suas ponderações (boost)
    var fieldsToSearch = new[]
    {
        "title^3",             // Peso maior para o campo título
        "body_content^2",      // Peso médio para o conteúdo do corpo
        "meta_keywords",       // Peso normal para meta palavras-chave
        "source^0.5",          // Peso menor para a fonte (URL original)
        "link^1"               // Peso normal para o link do item
    };

    // Executar a consulta de busca com paginação
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("search-news")
        .From(from)        // Paginação: ponto inicial
        .Size(pageSize)    // Paginação: número de itens por página
        .Query(q => q
            .Bool(b => b
                .Must(m => m
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(fieldsToSearch)
                        .Type(TextQueryType.MostFields)
                        .Fuzziness(Fuzziness.Auto) // Permite correção de erros de digitação
                        .Operator(Operator.And)    // Garante que todas as palavras da consulta estejam incluídas
                    )
                )
            )
        )
        .Suggest(su => su
            .Phrase("did_you_mean", ph => ph
                .Text(query)
                .Field("title")
                .Size(1)
                .DirectGenerator(dg => dg
                    .Field("title")
                    .SuggestMode(SuggestMode.Always)
                )
            )
        )
        .Highlight(h => h
            .PreTags("<em>")
            .PostTags("</em>")
            .MaxAnalyzedOffset(900000) // Define um valor menor que o limite do índice
            .Fields(
                f => f.Field("title"),
                f => f.Field("body_content"),
                f => f.Field("meta_keywords")
            )
        )
    );

    // Extrair a sugestão, se houver
    var suggestion = searchResponse.Suggest["did_you_mean"]
        .FirstOrDefault()?.Options?.FirstOrDefault()?.Text;

    // Preparar o resultado com detalhes de paginação
    var result = new
    {
      Total = searchResponse.Total,        // Número total de documentos que correspondem à consulta
      Page = page,                         // Página atual
      PageSize = pageSize,                 // Número de itens por página
      TotalPages = (int)Math.Ceiling((double)searchResponse.Total / pageSize), // Número total de páginas
      Results = searchResponse.Documents,  // Os documentos para a página atual
      Suggestions = suggestion != null && suggestion.ToLower() != query.ToLower() ? suggestion : null,
      Highlights = searchResponse.Hits.Select(hit => new
      {
        Id = hit.Id,
        Url = hit.Source,
        Highlights = hit.Highlight
      })
    };

    return Ok(result);
  }


  // Search with OpenAI query enhancement
  [HttpGet("search-enhanced")]
  public async Task<IActionResult> SearchWithEnhancedQuery(string query, int page = 1, int pageSize = 10)
  {
    // Use OpenAI to enhance the query
    var enhancedQuery = await _openAiClient.GenerateEnhancedQueryAsync(query);

    // Use the enhanced query in Elasticsearch
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("search-news")
        .From((page - 1) * pageSize)
        .Size(pageSize)
        .Query(q => q
            .Bool(b => b
                .Must(m => m
                    .MultiMatch(mm => mm
                        .Query(enhancedQuery)
                        .Fields(new[] { "title^3", "body^2", "tags", "url" })
                        .Type(TextQueryType.MostFields)
                        .Fuzziness(Fuzziness.Auto)
                        .Operator(Operator.And)
                    )
                )
            )
        )
        .Highlight(h => h
            .PreTags("<em>")
            .PostTags("</em>")
            .Fields(
                f => f.Field("title"),
                f => f.Field("body"),
                f => f.Field("tags")
            )
        )
    );

    return Ok(new
    {
      Total = searchResponse.Total,
      Results = searchResponse.Documents,
      EnhancedQuery = enhancedQuery,
      Highlights = searchResponse.Hits.Select(hit => new
      {
        Id = hit.Id,
        Source = hit.Source,
        Highlights = hit.Highlight
      })
    });
  }

  // Search with OpenAI-generated summaries
  [HttpGet("search-with-summary")]
  public async Task<IActionResult> SearchWithSummary(string query, int page = 1, int pageSize = 10)
  {
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("search-news")
        .From((page - 1) * pageSize)
        .Size(pageSize)
        .Query(q => q
            .Bool(b => b
                .Must(m => m
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(new[] { "title^3", "body^2", "tags", "url" })
                        .Type(TextQueryType.MostFields)
                        .Fuzziness(Fuzziness.Auto)
                        .Operator(Operator.And)
                    )
                )
            )
        )
    );

    // Summarize the top search results using OpenAI
    var summaries = await _openAiClient.GenerateSummariesAsync(searchResponse.Documents);

    return Ok(new
    {
      Total = searchResponse.Total,
      Results = searchResponse.Documents,
      Summaries = summaries
    });
  }

  // Determine if a query is a question and get a direct answer from OpenAI
  [HttpGet("search-or-answer")]
  public async Task<IActionResult> SearchOrAnswer(string query, int page = 1, int pageSize = 10)
  {
    var isQuestion = await _openAiClient.IsQuestionAsync(query);

    if (isQuestion)
    {
      // Get a direct answer from OpenAI
      var answer = await _openAiClient.AnswerQuestionAsync(query);
      return Ok(new { Answer = answer });
    }
    else
    {
      // Proceed with a regular search
      return await Search(query, page, pageSize);
    }
  }

 

  // Example 1: Search by Title
  [HttpGet("search-by-title")]
  public async Task<IActionResult> SearchByTitle(string title)
  {
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("web_articles")
        .Query(q => q
            .Match(m => m
                .Field("title")
                .Query(title)
            )
        )
    );

    return Ok(searchResponse.Documents);
  }

  // Example 2: Search by Content in Body
  [HttpGet("search-by-content")]
  public async Task<IActionResult> SearchByContent(string content)
  {
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("web_articles")
        .Query(q => q
            .Match(m => m
                .Field("body_content")
                .Query(content)
            )
        )
    );

    return Ok(searchResponse.Documents);
  }

  // Example 3: Search by URL
  [HttpGet("search-by-url")]
  public async Task<IActionResult> SearchByUrl(string url)
  {
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("web_articles")
        .Query(q => q
            .Term(t => t
                .Field("url.keyword")
                .Value(url)
            )
        )
    );

    return Ok(searchResponse.Documents);
  }

  // Example 4: Search by Title and Content
  [HttpGet("search-by-title-and-content")]
  public async Task<IActionResult> SearchByTitleAndContent(string title, string content)
  {
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("web_articles")
        .Query(q => q
            .Bool(b => b
                .Must(
                    m => m.Match(mm => mm.Field("title").Query(title)),
                    m => m.Match(mm => mm.Field("body_content").Query(content))
                )
            )
        )
    );

    return Ok(searchResponse.Documents);
  }


  public class PortfolioItem
  {
    public int Id { get; set; }
    public string OperationType { get; set; }
    public string UserId { get; set; }
    public int PortfolioId { get; set; }
    public string Symbol { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal Commission { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal CurrentMarketValue { get; set; }
    public decimal TotalDividendIncome { get; set; }

    public SocialSentimentData SocialSentiment { get; set; }
  }

  public class SocialSentimentData
  {
    public double PositiveScore { get; set; }
    public double NegativeScore { get; set; }
    public string Symbol { get; set; }
    public double SentimentScore { get; set; }
  }


  [HttpGet("portfolio")]
  public async Task<IActionResult> GetPortfolio( int page = 1, int pageSize = 50)
  {
    var userId = _userManager.GetUserId(User);
    if (string.IsNullOrWhiteSpace(userId))
      return BadRequest("userId é obrigatório.");

    var indexName = $"user-id-{userId}-portfolio";
    var from = (page - 1) * pageSize;

    var searchResponse = await _elasticClient.SearchAsync<PortfolioItem>(s => s
        .Index(indexName)
        .From(from)
        .Size(pageSize)
        .Query(q => q.MatchAll())
    );

    if (!searchResponse.IsValid)
    {
      return StatusCode(500, new
      {
        error = searchResponse.ServerError?.Error?.Reason,
        debug = searchResponse.DebugInformation
      });
    }

    return Ok(new
    {
      Total = searchResponse.Total,
      Page = page,
      PageSize = pageSize,
      Results = searchResponse.Documents
    });
  }
  // Autocomplete using OpenAI for enhanced suggestions
  [HttpGet("autocomplete")]
  public async Task<IActionResult> Autocomplete(string query, int maxSuggestions = 5)
  {
    var autocompleteResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("web_articles")
        .Size(maxSuggestions)
        .Query(q => q
            .MultiMatch(mm => mm
                .Query(query)
                .Fields(f => f
                    .Field("title")
                    .Field("tags")
                )
                .Type(TextQueryType.PhrasePrefix)
            )
        )
    );

    var suggestions = autocompleteResponse.Hits.Select(hit => hit.Source.title.ToString()).Distinct().ToList();

    return Ok(suggestions);
  }
  // Chat endpoint to handle different user inputs
  [HttpPost("chat")]
  public async Task<IActionResult> Chat([FromBody] ChatRequest request)
  {
    string userInput = request.Query;
    string response;

    // Determine if the user input is a question or should be enhanced
    var isQuestion = await _openAiClient.IsQuestionAsync(userInput);

    if (isQuestion)
    {
      // Get a direct answer from OpenAI
      response = await _openAiClient.AnswerQuestionAsync(userInput);
    }
    else
    {
      // Enhance the query using OpenAI
      var enhancedQuery = await _openAiClient.GenerateEnhancedQueryAsync(userInput);

      // Perform search based on enhanced query
      var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
          .Index("search-news")
          .From(0)
          .Size(5) // Limiting to top 5 results for simplicity
          .Query(q => q
              .Bool(b => b
                  .Must(m => m
                      .MultiMatch(mm => mm
                          .Query(enhancedQuery)
                          .Fields(new[] { "title^3", "body^2", "tags", "url" })
                          .Type(TextQueryType.MostFields)
                          .Fuzziness(Fuzziness.Auto)
                          .Operator(Operator.And)
                      )
                  )
              )
          )
          .Highlight(h => h
              .PreTags("<em>")
              .PostTags("</em>")
              .Fields(
                  f => f.Field("title"),
                  f => f.Field("body"),
                  f => f.Field("tags")
              )
          )
      );

      var summaries = await _openAiClient.GenerateSummariesAsync(searchResponse.Documents);

      response = FormatSearchResults(summaries);
    }

    return Ok(new { response });
  }

  private string FormatSearchResults(IEnumerable<string> summaries)
  {
    if (summaries == null || !summaries.Any())
    {
      return "I couldn't find any relevant results. Please try a different query.";
    }

    return string.Join("\n\n", summaries);
  }
  // Example 5: Search with Date Range
  [HttpGet("search-by-date-range")]
  public async Task<IActionResult> SearchByDateRange(string startDate, string endDate)
  {
    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("web_articles")
        .Query(q => q
            .DateRange(r => r
                .Field("last_crawled_at")
                .GreaterThanOrEquals(startDate)
                .LessThanOrEquals(endDate)
            )
        )
    );

    return Ok(searchResponse.Documents);
  }
}
