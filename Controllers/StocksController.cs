using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  [Route("stocks")]
  public class StocksController : Controller
  {
    private readonly MarketsService _SymbolsService;

    public StocksController(MarketsService SymbolsService)
    {
      _SymbolsService = SymbolsService;
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetSymbolsItem(int id)
    {
      var SymbolsItem = await _SymbolsService.GetSymbolsByIdAsync(id);
      if (SymbolsItem == null)
      {
        return NotFound();
      }
      return Json(SymbolsItem);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateSymbolsItem(int id, [FromBody] SymbolItem updatedSymbols)
    {
      if (id != updatedSymbols.Id)
      {
        return BadRequest();
      }

      var result = await _SymbolsService.UpdateSymbolsAsync(updatedSymbols);
      if (result)
      {
        return Json(new { success = true });
      }
      else
      {
        return Json(new { success = false, message = "Update failed" });
      }
    }
  }
}
