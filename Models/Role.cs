using System.ComponentModel.DataAnnotations;

namespace FopAccounting.Models;

public class Role
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string RoleName { get; set; } = string.Empty;
    public ICollection<UserProfileAccess> Accesses { get; set; } = new List<UserProfileAccess>();
    public override string ToString() => RoleName;
}
