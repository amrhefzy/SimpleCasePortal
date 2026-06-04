namespace SimpleCasePortal.Domain.Entities;

public sealed class RolePermission
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public Role Role { get; set; } = default!;

    public Permission Permission { get; set; } = default!;
}
