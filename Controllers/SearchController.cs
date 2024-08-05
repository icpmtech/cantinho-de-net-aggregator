using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{
  using AspnetCoreMvcFull.Services;
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Dynamic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  
  public class SearchController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly OpenAIService _openAIService;

    public SearchController(ApplicationDbContext context, OpenAIService openAIService)
    {
      _context = context;
      _openAIService = openAIService;
    }




    //[HttpGet]
    //public async Task<IActionResult> Search(string query)
    //{
    //  var model = new SearchViewModel
    //  {
    //    Query = query,
    //    Results = new List<StockEvent>()
    //  };

    //  if (!string.IsNullOrEmpty(query))
    //  {
    //    var openAIResponse = await _openAIService.GetSearchQueryResponse(query);
    //    model.Results = await _context.StockEvents
    //        .Where(item => item.Description.Contains(openAIResponse))
    //         .Where(item => item.Details.Contains(openAIResponse))
    //        .ToListAsync();

    //    if (model.Results.Any())
    //    {
    //      var contentToSummarize = new StringBuilder();
    //      foreach (var item in model.Results)
    //      {
    //        contentToSummarize.AppendLine(item.Description);
    //      }
    //      model.Summary = await _openAIService.SummarizeContent(contentToSummarize.ToString());
    //    }
    //  }

    //  return View(model);
    //}
    [HttpGet]
    public async Task<IActionResult> Index(string query)
    {
      var model = new SearchViewModel
      {
        Query = query,
        Results = new List<ExpandoObject>()
      };

      if (!string.IsNullOrEmpty(query))
      {
        try
        {
          var sqlQuery = await _openAIService.GenerateSQLQuery(query);
          model.SqlQuery = sqlQuery;

          if (!string.IsNullOrEmpty(sqlQuery))
          {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
              command.CommandText = sqlQuery;
              _context.Database.OpenConnection();

              using (var result = await command.ExecuteReaderAsync())
              {
                while (await result.ReadAsync())
                {
                  var row = new ExpandoObject() as IDictionary<string, object>;

                  for (int i = 0; i < result.FieldCount; i++)
                  {
                    row.Add(result.GetName(i), result.GetValue(i));
                  }

                  model.Results.Add((ExpandoObject)row);
                }
              }
            }

            if (model.Results.Any())
            {
              var contentToSummarize = new StringBuilder();
              foreach (IDictionary<string, object> item in model.Results)
              {
                if (item.ContainsKey("Details"))
                {
                  contentToSummarize.AppendLine(item["Details"].ToString());
                }
                if (item.ContainsKey("Description"))
                {
                  contentToSummarize.AppendLine(item["Description"].ToString());
                }
              }
              model.Summary += await _openAIService.SummarizeContent(contentToSummarize.ToString());
            }
          }
        }
        catch (Exception ex)
        {
          model.Error = $"An error occurred: {ex.Message}";
        }
      }

      return View(model);
    }
  }

}
