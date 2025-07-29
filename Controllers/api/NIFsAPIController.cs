using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace MarketAnalyticHub.Controllers.Api;

[ApiController]
[Route("api/nifs")]
public class NifsController : ControllerBase
{
  private readonly IHttpClientFactory _factory;

 
    public NifsController(IHttpClientFactory factory) => _factory = factory;

    [HttpGet("{query}")]
    public async Task<ActionResult<List<RaciusHit>>> Get(string query, CancellationToken ct)
    {
      var cli = _factory.CreateClient("racius");

      // 1º pedido só para obter os cookies cf_clearance
      await cli.GetAsync("/", ct);

      var url = $"/pesquisa/?json=1&q={Uri.EscapeDataString(query)}";
      var hits = await cli.GetFromJsonAsync<List<RaciusHit>>(url, ct);

      if (hits is null || hits.Count == 0)
        return NotFound("Nada encontrado na Racius.");

      return hits;                 
    }
}


