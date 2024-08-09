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

  // Google-like Search with typo correction, boosting, highlighting, and pagination
  [HttpGet("search")]
  public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 10)
  {
    // Define fields to search and boost
    var fieldsToSearch = new[] {
            "title^3",     // Boost title field
            "body^2",      // Boost body field
            "tags",        // Normal weight for tags
            "url"          // Less weight for URL
        };

    var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
        .Index("search-news")
        .From((page - 1) * pageSize) // Pagination
        .Size(pageSize)
        .Query(q => q
            .Bool(b => b
                .Must(m => m
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(fieldsToSearch)
                        .Type(TextQueryType.MostFields)
                        .Fuzziness(Fuzziness.Auto) // Allows for typo correction
                        .Operator(Operator.And) // Makes sure all words in the query are included
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
            .Fields(
                f => f.Field("title"),
                f => f.Field("body"),
                f => f.Field("tags")
            )
        )
    );

    // Extract the suggestion if any
    var suggestion = searchResponse.Suggest["did_you_mean"]
        .FirstOrDefault()?.Options?.FirstOrDefault()?.Text;

    var result = new
    {
      Total = searchResponse.Total,
      Results = searchResponse.Documents,
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
