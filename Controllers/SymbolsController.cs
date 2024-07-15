using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class SymbolsController : Controller
  {
    private readonly ISymbolService _symbolService;

    public SymbolsController(ISymbolService symbolService)
    {
      _symbolService = symbolService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSymbols()
    {
      var symbols = await _symbolService.GetSymbolsAsync();
      return Ok(symbols);
    }
  }

}
