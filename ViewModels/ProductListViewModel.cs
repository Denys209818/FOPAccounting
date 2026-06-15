using FopAccounting.Models;

namespace FopAccounting.ViewModels;

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> Warehouses { get; set; } = new() { "Основний склад" };
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Warehouse { get; set; }
    public string? Sort { get; set; }
    public bool LowStockOnly { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public decimal PotentialIncome { get; set; }
}
