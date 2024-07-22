
namespace MarketAnalyticHub
{

  public class FileUploadRequest
  {
    public string UploaderName { get; set; }
    public string UploaderAddress { get; set; }
    public IFormFile File { get; set; }
  }
}
