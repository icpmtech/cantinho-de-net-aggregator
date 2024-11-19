using DocumentFormat.OpenXml.Presentation;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using Azure;
namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/analizer-symbol-360")]
  public class AnalizerSymbol360Controller : ControllerBase
  {

    private readonly HttpClient _httpClient;

    public AnalizerSymbol360Controller(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    [HttpGet("compet-glance-analysis/{symbol}")]
    public async Task<IActionResult> GetCompetitorData(string symbol)
    {
      var url = $"https://csimarket.com/stocks/compet_glance.php?code={symbol}";

      try
      {
        // Fetch the HTML content from the URL
        var responseMessage = await _httpClient.GetAsync(url);
        responseMessage.EnsureSuccessStatusCode(); // Ensure the request was successful

        // Read the content as a byte array and decode it with UTF-8
        var responseBytes = await responseMessage.Content.ReadAsByteArrayAsync();
        var responseContent = Encoding.UTF8.GetString(responseBytes);

        // Load HTML into HtmlAgilityPack for parsing
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);

        // Extract data
        var competitorData = new ModelCompetitorData
        {
          Symbol = symbol,
          CompanyName = ExtractCompanyName(htmlDoc),
          Sector = ExtractSector(htmlDoc),
          Industry = ExtractIndustry(htmlDoc),
          FinancialTable = ExtractFinancialCompetitorDataTable(htmlDoc)
        };

        return Ok(competitorData);
      }
      catch (HttpRequestException)
      {
        return StatusCode(500, "Error fetching data from csimarket.com");
      }
    }
    private List<CompetitorData> ExtractFinancialCompetitorDataTable(HtmlDocument htmlDoc)
    {
      var financialDataList = new List<CompetitorData>();

      // Locate the rows of the table containing financial data
      var tableRows = htmlDoc.DocumentNode.SelectNodes("//table[@class='osnovna_tablica_bez_gifa']//tr[position() > 1]");
      if (tableRows != null)
      {
        foreach (var row in tableRows)
        {
          var cells = row.SelectNodes("td");
          if (cells != null && cells.Count >= 5)
          {
            var data = new CompetitorData
            {
              CompanyName = cells[0].InnerText.Trim(),
              MarketCap = cells[1].InnerText.Trim(),
              Revenues = cells[2].InnerText.Trim(),
              Income = cells[3].InnerText.Trim(),
              Employees = cells[4].InnerText.Trim()
            };
            financialDataList.Add(data);
          }
        }
      }

      return financialDataList;
    }
    [HttpGet("customer-analysis/{symbol}")]
    public async Task<IActionResult> GetCustomerData(string symbol)
    {
      var url = $"https://csimarket.com/stocks/markets_glance.php?code={symbol}";

      try
      {
        // Fetch the HTML content from the URL
        var responseMessage = await _httpClient.GetAsync(url);
        responseMessage.EnsureSuccessStatusCode(); // Ensure the request was successful

        // Read the content as a byte array and decode it with UTF-8
        var responseBytes = await responseMessage.Content.ReadAsByteArrayAsync();
        var responseContent = Encoding.UTF8.GetString(responseBytes);

        // Load HTML into HtmlAgilityPack for parsing
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);

        // Extract data
        var customerData = new ModelCustomerData
        {
          Symbol = symbol,
          CompanyName = ExtractCompanyName(htmlDoc),
          Sector = ExtractSector(htmlDoc),
          Industry = ExtractIndustry(htmlDoc),
          FinancialTable = ExtractFinancialTable(htmlDoc),
          RevenueByDivision = ExtractRevenueByDivision(htmlDoc),
          RevenueGrowthByIndustry = ExtractRevenueGrowthByIndustry(htmlDoc),
          CustomerPerformanceMetrics = ExtractCustomerPerformanceMetrics(htmlDoc)
        };

        return Ok(customerData);
      }
      catch (HttpRequestException)
      {
        return StatusCode(500, "Error fetching data from csimarket.com");
      }
    }
    private List<FinancialData> ExtractFinancialTable(HtmlDocument htmlDoc)
    {
      var financialDataList = new List<FinancialData>();

      // Locate the table rows (skip the header row)
      var tableRows = htmlDoc.DocumentNode.SelectNodes("//table[@class='osnovna_tablica_bez_gifa']//tr[position() > 1]");
      if (tableRows != null)
      {
        foreach (var row in tableRows)
        {
          var cells = row.SelectNodes("td");
          if (cells != null && cells.Count >= 5)
          {
            var financialData = new FinancialData
            {
              CompanyName = cells[0].InnerText.Trim(),
              MarketCap = cells[1].InnerText.Trim(),
              Revenues = cells[2].InnerText.Trim(),
              Income = cells[3].InnerText.Trim(),
              Employees = cells[4].InnerText.Trim()
            };

            financialDataList.Add(financialData);
          }
        }
      }

      return financialDataList;
    }
    private string ExtractCompanyName(HtmlDocument doc)
    {
      var node = doc.DocumentNode.SelectSingleNode("//div[@class='naslovcompet3']/h1");
      return node?.InnerText.Trim() ?? "N/A";
    }

    private string ExtractSector(HtmlDocument doc)
    {
      var node = doc.DocumentNode.SelectSingleNode("//span[@class='sector']");
      return node?.InnerText.Trim() ?? "N/A";
    }

    private string ExtractIndustry(HtmlDocument doc)
    {
      var node = doc.DocumentNode.SelectSingleNode("//span[@class='industry']");
      return node?.InnerText.Trim() ?? "N/A";
    }

    private Dictionary<string, string> ExtractRevenueByDivision(HtmlDocument doc)
    {
      var divisions = new Dictionary<string, string>();
      var divisionNodes = doc.DocumentNode.SelectNodes("//table//tr[@onmouseover='this.className=\"bgplv\"']");
      if (divisionNodes != null)
      {
        foreach (var node in divisionNodes)
        {
          var divisionName = node.SelectSingleNode(".//td[@class='segtxt']")?.InnerText.Trim();
          var revenuePercentage = node.SelectSingleNode(".//td[@class='deschardistance']")?.InnerText.Trim();

          if (!string.IsNullOrEmpty(divisionName) && !string.IsNullOrEmpty(revenuePercentage))
          {
            divisions[divisionName] = revenuePercentage;
          }
        }
      }
      return divisions;
    }
    private List<IndustryGrowth> ExtractRevenueGrowthByIndustry(HtmlDocument doc)
    {
      var growthData = new List<IndustryGrowth>();
      var industryNodes = doc.DocumentNode.SelectNodes("//table//tr[@valign='middle']");
      if (industryNodes != null)
      {
        foreach (var node in industryNodes)
        {
          var industryName = node.SelectSingleNode(".//td[@class='segtxt']/a")?.InnerText.Trim();
          var growthPercentage = node.SelectSingleNode(".//td[@class='lijevchar b lijevcharfont']")?.InnerText.Trim();

          if (!string.IsNullOrEmpty(industryName) && !string.IsNullOrEmpty(growthPercentage))
          {
            growthData.Add(new IndustryGrowth
            {
              Industry = industryName,
              GrowthPercentage = growthPercentage
            });
          }
        }
      }
      return growthData;
    }

    private CustomerPerformanceMetrics ExtractCustomerPerformanceMetrics(HtmlDocument doc)
    {
      var netIncomeGrowth = doc.DocumentNode.SelectSingleNode("//span[@class='zv']")?.InnerText.Trim() ?? "N/A";
      var netMarginGrowth = doc.DocumentNode.SelectSingleNode("//span[@class='zv']")?.InnerText.Trim() ?? "N/A";
      return new CustomerPerformanceMetrics
      {
        NetIncomeGrowth = netIncomeGrowth,
        NetMarginGrowth = netMarginGrowth
      };
    }



  }
}
