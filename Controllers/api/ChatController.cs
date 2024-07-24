using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using MarketAnalyticHub;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using OpenAI_API;
using OpenAI_API.Audio;
using static OpenAI_API.Audio.TextToSpeechRequest;
using OpenAI_API.Models;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  private readonly IWebHostEnvironment _environment;
  private readonly IHttpClientFactory _clientFactory;
  private readonly IConfiguration _configuration;
  private readonly ILogger<ChatController> _logger;
  private readonly OpenAIAPI _openAiApi;
  public ChatController(
      ILogger<ChatController> logger,
      IWebHostEnvironment environment,
      IHttpClientFactory clientFactory,
      IConfiguration configuration)
  {
    _environment = environment;
    _logger = logger;
    _clientFactory = clientFactory;
    _configuration = configuration;
    _openAiApi = new OpenAIAPI(_configuration["OpenAI:ApiKey"]);
  }

  [HttpPost("UploadFile")]
  public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest model)
  {
    if (model.File == null || model.File.Length == 0)
    {
      return BadRequest(new { success = false, message = "Please upload a valid file." });
    }

    var filePath = Path.Combine(Path.GetTempPath(), model.File.FileName);

    try
    {
      // Save the uploaded file to a temporary path
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await model.File.CopyToAsync(stream);
      }

      // Encode the file to base64
      var base64File = EncodeFileToBase64(filePath);

      string fileType = model.File.ContentType.Split('/')[0];

      // Prepare the payload for OpenAI API
      var payload = new
      {
        model = "gpt-4o",
        messages = new[]
          {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "What’s in this file?" },
                            new { type = "image_url", image_url  = new { url = $"data:{model.File.ContentType};base64,{base64File}" } }
                        }
                    }
                },
        max_tokens = 300
      };

      var jsonPayload = JsonSerializer.Serialize(payload);
      var client = _clientFactory.CreateClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);
      var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync();
        return BadRequest(new { success = false, message = "Error analyzing file with OpenAI", errorContent });
      }

      var resultContent = await response.Content.ReadAsStringAsync();
      var analysis = JsonDocument.Parse(resultContent).RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString().Trim();

      return Ok(new { success = true, analysis });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error uploading or analyzing file.");
      return StatusCode(500, new { success = false, message = "Internal server error." });
    }
  }

  [HttpPost("ReadAudio")]
  public async Task<IActionResult> ReadAudio([FromBody] MessageContent content)
  {
    var request = new TextToSpeechRequest()
    {
      Input = content.Content,
      ResponseFormat = ResponseFormats.AAC,
      Model = Model.TTS_HD,
      Voice = Voices.Nova,
      Speed = 0.9
    };

    var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.aac");
    await _openAiApi.TextToSpeech.SaveSpeechToFileAsync(request, filePath);

    var fileInfo = new FileInfo(filePath);
    if (!fileInfo.Exists)
    {
      return NotFound();
    }

    var memory = new MemoryStream();
    using (var stream = new FileStream(filePath, FileMode.Open))
    {
      await stream.CopyToAsync(memory);
    }
    memory.Position = 0;

    return File(memory, "audio/aac", fileInfo.Name);
  }



  [HttpPost("UploadAudio")]
  public async Task<IActionResult> UploadAudio([FromForm] FileUploadRequest model)
  {
    if (model.File == null || model.File.Length == 0)
    {
      return BadRequest(new { success = false, message = "Please upload a valid file." });
    }

    var filePath = Path.Combine(Path.GetTempPath(), model.File.FileName+ ".wav");

    try
    {
      // Save the uploaded file to a temporary path
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await model.File.CopyToAsync(stream);
      }

      // Transcribe the audio file using the OpenAI Whisper API
      var transcription = await TranscribeAudio(filePath);

      if (transcription == null)
      {
        return BadRequest(new { success = false, message = "Error transcribing audio file." });
      }

      return Ok(new { success = true, transcription });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error uploading or transcribing audio.");
      return StatusCode(500, new { success = false, message = "Internal server error." });
    }
  }

  private async Task<string> TranscribeAudio(string filePath)
  {
    try
    {
      var transcription = await _openAiApi.Transcriptions.GetTextAsync(filePath);

      return transcription;
     
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error transcribing audio.");
      return null;
    }
  }

  private string EncodeFileToBase64(string filePath)
  {
    byte[] fileArray = System.IO.File.ReadAllBytes(filePath);
    return Convert.ToBase64String(fileArray);
  }

  [HttpPost("saveMessage")]
  public async Task<IActionResult> SaveMessage([FromBody] MessageContent content)
  {
    var uploadsFolder = Path.Combine(_environment.WebRootPath, "wwwroot", "uploads", "savedMessages");
    if (!Directory.Exists(uploadsFolder))
    {
      Directory.CreateDirectory(uploadsFolder);
    }

    var filePath = Path.Combine(uploadsFolder, $"{Guid.NewGuid()}.txt");
    await System.IO.File.WriteAllTextAsync(filePath, content.Content);

    return Ok(new { filePath });
  }

  public class MessageContent
  {
    public string Content { get; set; }
  }
}
