

using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.api
{

  [ApiController]
  [Route("api/[controller]")]
  public class YahooFinancev1Controller : ControllerBase
  {
    /// <summary>
    /// Obtém dados históricos de uma ação em um intervalo de datas específico.
    /// </summary>
    /// <param name="symbol">Símbolo da ação (ex: AAPL).</param>
    /// <param name="startDate">Data de início.</param>
    /// <param name="endDate">Data de fim.</param>
    /// <param name="interval">Intervalo de dados (ex: 1d, 1wk, 1mo). Padrão: 1d.</param>
    /// <returns>Dados históricos da ação.</returns>
    [HttpGet("GetHistoricalData")]
    public async Task<IActionResult> GetHistoricalData(
        [FromQuery] string symbol,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval = "1d",
        CancellationToken token = default)
    {
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest(new { Message = "Symbol is required." });
      }

      if (endDate < startDate)
      {
        return BadRequest(new { Message = "End date must be greater than or equal to start date." });
      }

      try
      {
        var historicalData = await YahooService.GetHistoricalJsonDataAsync(symbol, startDate , endDate, token);

        if (historicalData == null )
        {
          return Ok(new { Message = "No historical data found.", Data = (object)null });
        }

       

        return Ok(historicalData);
      }
      catch (Exception ex)
      {
        // Log the exception details as needed
        Console.WriteLine($"Error in GetHistoricalData: {ex.Message}");
        return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
      }
    }

    /// <summary>
    /// Obtém o preço atual de uma ação.
    /// </summary>
    /// <param name="symbol">Símbolo da ação (ex: AAPL).</param>
    /// <returns>Dados de preço atual da ação.</returns>
    [HttpGet("GetCurrentPrice")]
    public async Task<IActionResult> GetCurrentPrice([FromQuery] string symbol, CancellationToken token = default)
    {
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest(new { Message = "Symbol is required." });
      }

      try
      {
        var currentData = await YahooService.GetCurrentDataAsync(symbol, token);

        if (currentData == null)
        {
          return Ok(new { Message = "No current data found.", Data = (object)null });
        }

        return Ok(currentData);
      }
      catch (Exception ex)
      {
        // Log the exception details as needed
        Console.WriteLine($"Error in GetCurrentPrice: {ex.Message}");
        return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
      }
    }

    /// <summary>
    /// Obtém informações da indústria para um símbolo de ação.
    /// </summary>
    /// <param name="symbol">Símbolo da ação (ex: AAPL).</param>
    /// <returns>Nome da indústria da ação.</returns>
    [HttpGet("GetIndustry")]
    public async Task<IActionResult> GetIndustry([FromQuery] string symbol, CancellationToken token = default)
    {
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest(new { Message = "Symbol is required." });
      }

      try
      {
        var industry = await YahooService.GetIndustryBySymbolAsync(symbol, token);

        return Ok(new { Symbol = symbol.ToUpper(), Industry = industry });
      }
      catch (Exception ex)
      {
        // Log the exception details as needed
        Console.WriteLine($"Error in GetIndustry: {ex.Message}");
        return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
      }
    }

    /// <summary>
    /// Pesquisa símbolos de ações com base em uma consulta.
    /// </summary>
    /// <param name="query">Termo de pesquisa.</param>
    /// <returns>Lista de símbolos que correspondem à consulta.</returns>
    [HttpGet("SearchSymbols")]
    public async Task<IActionResult> SearchSymbols([FromQuery] string query, CancellationToken token = default)
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        return BadRequest(new { Message = "Query parameter is required." });
      }

      try
      {
        var symbols = await YahooService.SearchSymbolsAsync(query, token);

        if (symbols == null || !symbols.Any())
        {
          return Ok(new { Message = "No symbols found for the given query.", Data = (object)null });
        }

        return Ok(symbols);
      }
      catch (Exception ex)
      {
        // Log the exception details as needed
        Console.WriteLine($"Error in SearchSymbols: {ex.Message}");
        return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
      }
    }
  }
}
