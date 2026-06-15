using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FopAccounting.Models;

public class StockOperation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    [Required, StringLength(50)] public string OperationType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    [StringLength(255)] public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Product? Product { get; set; }

    [NotMapped] public string Type { get => OperationType; set => OperationType = value; }
    [NotMapped] public DateTime Date { get => CreatedAt; set => CreatedAt = value; }
    [NotMapped] public string ProductName => Product?.Name ?? "-";
    [NotMapped] public string Counterparty { get; set; } = "-";
    [NotMapped] public decimal Amount => Quantity * (Product?.PurchasePrice ?? 0);
}
