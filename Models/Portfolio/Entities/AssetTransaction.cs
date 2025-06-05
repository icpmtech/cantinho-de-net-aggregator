using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MarketAnalyticHub.Models.Portfolio.Entities
{
  [Table("AssetTransactionUsers")]
  public class AssetTransactionUser
  {
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Required]
    [MaxLength(10)]
    public string Ticker { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BuyPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SellPrice { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [MaxLength(20)]
    public string Type { get; set; }     // Compra, Venda, Dividendos, Outros

    [Required]
    [MaxLength(10)]
    public string Status { get; set; }   // Aberto / Fechado

    [MaxLength(250)]
    public string Notes { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; }

   
  }
}
