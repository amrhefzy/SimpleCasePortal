namespace SimpleCasePortal.Domain.Entities;

public sealed class UserRole
{
    public string UserId { get; set; } = default!;

    public int RoleId { get; set; }

    public ApplicationUser User { get; set; } = default!;

    public Role Role { get; set; } = default!;
}
