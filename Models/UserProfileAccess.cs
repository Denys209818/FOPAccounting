namespace FopAccounting.Models;

public class UserProfileAccess
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProfileId { get; set; }
    public int RoleId { get; set; }

    public User? User { get; set; }
    public BusinessProfile? Profile { get; set; }
    public Role? Role { get; set; }
}
