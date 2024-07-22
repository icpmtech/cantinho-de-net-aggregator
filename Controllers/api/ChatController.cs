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

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
  private readonly IWebHostEnvironment _environment;
  private readonly IHttpClientFactory _clientFactory;
  private readonly IConfiguration _configuration;
  private readonly ILogger<ChatController> _logger;

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

  private string EncodeFileToBase64(string filePath)
  {
    byte[] fileArray = System.IO.File.ReadAllBytes(filePath);
    return Convert.ToBase64String(fileArray);
  }

  [HttpPost("saveMessage")]
  public async Task<IActionResult> SaveMessage([FromBody] MessageContent content)
  {
    var filePath = Path.Combine(_environment.WebRootPath, "savedMessages", $"{Guid.NewGuid()}.txt");
    await System.IO.File.WriteAllTextAsync(filePath, content.Content);

    return Ok(new { filePath });
  }

  public class MessageContent
  {
    public string Content { get; set; }
  }
}
