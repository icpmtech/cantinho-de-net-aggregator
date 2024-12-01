using AspnetCoreMvcFull.Services;
using Elasticsearch.Net;
using MarketAnalyticHub.Services.Elastic;
using Microsoft.AspNetCore.Mvc;
using Nest;


[Route("api/[controller]")]
[ApiController]
public class SearchController : ControllerBase
{
  private readonly ElasticClient _elasticClient;
  private readonly OpenAIService _openAiClient;

  public SearchController(ElasticSearchService elasticClient, OpenAIService openAiClient)
  {
    _elasticClient = elasticClient._client;
    _openAiClient = openAiClient;
  }

  [HttpGet("search")]
  public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 10)
  {
    // Garantir que a página seja pelo menos 1 para evitar problemas com páginas negativas ou zero
    if (page < 1) page = 1;

    // Calcular o ponto inicial para paginação
    var from = (page - 1) * pageSize;

    // Campos a serem buscados e suas ponderações (boost)
    var fieldsToSearch = new[]
    {
        "text^3",               // Maior peso para o texto extraído
        "href^1",               // Peso menor para URLs
        "title^2",              // Peso médio para títulos (se disponíveis)
        "source"                // Peso neutro para origem (URL base)
    };

    try
    {
      // Executar a busca no Elasticsearch com paginação
      var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
          .Index("search-news")  // Substituir pelo nome real do índice, se diferente
          .From(from)                // Paginação: ponto inicial
          .Size(pageSize)            // Paginação: itens por página
          .Query(q => q
              .Bool(b => b
                  .Must(m => m
                      .MultiMatch(mm => mm
                          .Query(query)
                          .Fields(fieldsToSearch)
                          .Type(TextQueryType.MostFields) // Busca em múltiplos campos
                          .Fuzziness(Fuzziness.Auto)      // Correção de erros de digitação
                          .Operator(Operator.And)         // Garante que todas as palavras sejam incluídas
                      )
                  )
              )
          )
          .Suggest(su => su
              .Phrase("did_you_mean", ph => ph
                  .Text(query)
                  .Field("text")     // Sugestões baseadas no campo principal
                  .Size(1)
                  .DirectGenerator(dg => dg
                      .Field("text")
                      .SuggestMode(SuggestMode.Always) // Sempre sugere correções
                  )
              )
          )
          .Highlight(h => h
              .PreTags("<em>")     // Tag de abertura para destaque
              .PostTags("</em>")   // Tag de fechamento para destaque
              .Fields(
                  f => f.Field("text"),
                  f => f.Field("href"),
                  f => f.Field("title"),
                  f => f.Field("source")
              )
          )
      );

      // Extrair sugestões, se houver
      var suggestion = searchResponse.Suggest["did_you_mean"]
          .FirstOrDefault()?.Options?.FirstOrDefault()?.Text;

      // Preparar os resultados com detalhes de paginação
      var result = new
      {
        Total = searchResponse.Total,        // Total de documentos que correspondem à consulta
        Page = page,                         // Página atual
        PageSize = pageSize,                 // Itens por página
        TotalPages = (int)Math.Ceiling((double)searchResponse.Total / pageSize), // Total de páginas
        Results = searchResponse.Documents,  // Documentos da página atual
        Suggestions = suggestion != null && suggestion.ToLower() != query.ToLower() ? suggestion : null,
        Highlights = searchResponse.Hits.Select(hit => new
        {
          Id = hit.Id,
          Source = hit.Source,
          Highlights = hit.Highlight
        })
      };

      return Ok(result);
    }
    catch (Exception ex)
    {
      // Retornar erro com detalhes
      return StatusCode(500, new
      {
        Error = "Erro ao processar a busca",
        Details = ex.Message
      });
    }
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
