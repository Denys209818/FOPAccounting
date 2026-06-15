using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FopAccounting.Models;

public class BusinessProfile
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string ProfileName { get; set; } = string.Empty;
    [StringLength(50)] public string? BusinessType { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [NotMapped] public string Name { get => ProfileName; set => ProfileName = value; }
    [NotMapped] public string? ActivityType { get => BusinessType; set => BusinessType = value; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? TaxNumber { get; set; }

    public User? Owner { get; set; }
    public ICollection<UserProfileAccess> Accesses { get; set; } = new List<UserProfileAccess>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ManagementOperation> ManagementOperations { get; set; } = new List<ManagementOperation>();
}
