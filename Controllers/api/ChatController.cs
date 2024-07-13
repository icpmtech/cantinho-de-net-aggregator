
namespace AspnetCoreMvcFull.Controllers.api
{
  using Microsoft.AspNetCore.Mvc;
  using System;
  using System.IO;
  using System.Threading.Tasks;
  [ApiController]
  [Route("api/[controller]")]
  public class ChatController : ControllerBase
  {
    [HttpPost("saveMessage")]
    public async Task<IActionResult> SaveMessage([FromBody] ChatMessage message)
    {
      if (message == null || string.IsNullOrWhiteSpace(message.Content))
      {
        return BadRequest("Message content is required");
      }

      var filePath = Path.Combine("SavedMessages", $"{Guid.NewGuid()}.txt");
      Directory.CreateDirectory(Path.GetDirectoryName(filePath));

      await System.IO.File.WriteAllTextAsync(filePath, message.Content);

      return Ok(new { FilePath = filePath });
    }
  }

  public class ChatMessage
  {
    public string Content { get; set; }
  }
}
