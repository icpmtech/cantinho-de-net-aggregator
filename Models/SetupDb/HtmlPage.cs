using System.ComponentModel.DataAnnotations;

namespace MarketAnalyticHub.Models.SetupDb
{
  public class HtmlPage
  {
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    public string Slug { get; set; }

    [Required]
    public string Content { get; set; }
    public string? MetaTitle { get;  set; }
    public string? MetaDescription { get;  set; }
    public string? Keywords { get;  set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? LastEditedBy { get;  set; }
    public string? ChangeHistory { get;  set; }
  }
}
