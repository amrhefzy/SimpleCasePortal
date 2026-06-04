namespace SimpleCasePortal.Domain.Entities;

public sealed class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
