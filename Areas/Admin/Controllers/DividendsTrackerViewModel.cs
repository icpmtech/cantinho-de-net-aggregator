using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarketAnalyticHub.Models
{
  using System.ComponentModel.DataAnnotations;

  public class DividendsTrackerViewModel
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    public string Company { get; set; }

    [Required(ErrorMessage = "Ticker is required.")]
    public string Ticker { get; set; }

    [Required(ErrorMessage = "Country is required.")]
    public string Country { get; set; }

    [Required(ErrorMessage = "Region is required.")]
    public string Region { get; set; }

    [Required(ErrorMessage = "Exchange is required.")]
    public string Exchange { get; set; }

    [Required(ErrorMessage = "Share Price is required.")]
   
    public string SharePrice { get; set; }

    [Required(ErrorMessage = "Previous Dividend is required.")]
    public string PrevDividend { get; set; }

    [Required(ErrorMessage = "Ex Date Dividend is required.")]
    public DateTime? ExDateDividend { get; set; }

    [Required(ErrorMessage = "Pay Date Dividend is required.")]
    public DateTime? PayDateDividend { get; set; }

    public int[] SelectedIndices { get; set; }
    public IEnumerable<SelectListItem>? AvailableIndices { get; set; }
  }



}
