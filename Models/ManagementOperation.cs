using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FopAccounting.Models;

public class ManagementOperation
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [Required, StringLength(50)] public string OperationType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    [StringLength(255)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public BusinessProfile? Profile { get; set; }

    [NotMapped] public string Type { get => OperationType; set => OperationType = value; }
    [NotMapped] public string? Comment { get => Description; set => Description = value; }
    [NotMapped] public DateTime Date { get => CreatedAt; set => CreatedAt = value; }
}
