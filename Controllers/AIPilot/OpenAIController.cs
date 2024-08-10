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
}

public class ChatRequest
{
  public string Query { get; set; }
}


