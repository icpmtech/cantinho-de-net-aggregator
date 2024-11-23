using System.ComponentModel.DataAnnotations;

namespace MarketAnalyticHub.Controllers.api
{
  public class PortfolioIdDto
  {
    public class PortfolioAnalysisDto
    {
      [Required(ErrorMessage = "Portfolio ID is required.")]
      [Range(1, int.MaxValue, ErrorMessage = "Portfolio ID must be a positive integer.")]
      public int PortfolioId { get; set; }

      // Optional Prompt Text
      public string? PromptInput { get; set; }
    }
  }
}
