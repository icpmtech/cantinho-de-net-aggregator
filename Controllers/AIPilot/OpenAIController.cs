using AspnetCoreMvcFull.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/openai")]
[ApiController]
public class OpenAIController : ControllerBase
{
  private readonly OpenAIService _openAiClient;
  public OpenAIController(OpenAIService openAiClient)
  {
    _openAiClient = openAiClient;
  }

  [HttpPost("chat")]
  public async Task<IActionResult> Chat([FromBody] ChatRequest request)
  {
    var response = await _openAiClient.GetChatResponseAsync(request.Query);
    return Ok(new { response });
  }


  /// <summary>
  /// Transcreve áudio para texto via OpenAI Whisper.
  /// </summary>
  /// <remarks>
  /// Envie o ficheiro em <b>multipart/form-data</b>.
  /// </remarks>
  [HttpPost("transcribe")]
  [Consumes("multipart/form-data")]                    // ①
  public async Task<IActionResult> Transcribe(
     )
  {
    // … usar audio.OpenReadStream() …
    return Ok(new { text = "Olá mundo!" });
  }
}
  public class ChatRequest
{
  public string Query { get; set; }
}


