using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class QualitativeEventsController : Controller
  {
    private readonly IQualitativeEventService _qualitativeEventService;

    public QualitativeEventsController(IQualitativeEventService qualitativeEventService)
    {
      _qualitativeEventService = qualitativeEventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetQualitativeEvents()
    {
      var events = await _qualitativeEventService.GetQualitativeEventsAsync();
      return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQualitativeEvent(int id)
    {
      var qualitativeEvent = await _qualitativeEventService.GetQualitativeEventByIdAsync(id);
      if (qualitativeEvent == null)
      {
        return NotFound();
      }
      return Ok(qualitativeEvent);
    }

    [HttpPost]
    public async Task<IActionResult> AddQualitativeEvent(QualitativeEvent qualitativeEvent)
    {
      await _qualitativeEventService.AddQualitativeEventAsync(qualitativeEvent);
      return CreatedAtAction(nameof(GetQualitativeEvent), new { id = qualitativeEvent.Id }, qualitativeEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQualitativeEvent(int id, QualitativeEvent qualitativeEvent)
    {
      if (id != qualitativeEvent.Id)
      {
        return BadRequest();
      }
      await _qualitativeEventService.UpdateQualitativeEventAsync(qualitativeEvent);
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQualitativeEvent(int id)
    {
      await _qualitativeEventService.DeleteQualitativeEventAsync(id);
      return NoContent();
    }
  }

}
