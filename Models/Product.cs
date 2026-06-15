using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FopAccounting.Models;

public class Product
{
    public int Id { get; set; }
    public int ProfileId { get; set; }

    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    [StringLength(100)] public string? Category { get; set; }
    [StringLength(50)] public string? Article { get; set; }
    public int Quantity { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public int MinStock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [NotMapped] public string? Sku { get => Article; set => Article = value; }
    [NotMapped] public int MinQuantity { get => MinStock; set => MinStock = value; }
    [NotMapped] public string Unit { get; set; } = "шт.";
    [NotMapped] public string Warehouse { get; set; } = "Основний склад";
    [NotMapped] public decimal Margin => (SalePrice ?? 0) - (PurchasePrice ?? 0);
    [NotMapped] public bool IsLowStock => Quantity <= MinStock;

    public BusinessProfile? Profile { get; set; }
    public ICollection<StockOperation> StockOperations { get; set; } = new List<StockOperation>();
}
