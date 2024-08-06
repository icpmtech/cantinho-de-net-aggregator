using MarketAnalyticHub.Controllers;
using System.Dynamic;

namespace MarketAnalyticHub.Models
{
  public class SearchViewModel
  {
    public string Query { get; set; }
    public string SqlQuery { get; set; }
    public List<ExpandoObject> Results { get; set; }
    public string Summary { get; set; }
    public string Error { get; set; }
  }
}
