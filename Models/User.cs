using System.ComponentModel.DataAnnotations;

namespace FopAccounting.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<BusinessProfile> BusinessProfiles { get; set; } = new List<BusinessProfile>();
    public ICollection<UserProfileAccess> Accesses { get; set; } = new List<UserProfileAccess>();
}
