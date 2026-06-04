using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Security;

public sealed class PermissionService : IPermissionService
{
    private readonly AppDbContext _dbContext;

    public PermissionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permissionName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .AnyAsync(rolePermission => rolePermission.Permission.Name == permissionName, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.Name)
            .Distinct()
            .OrderBy(permissionName => permissionName)
            .ToArrayAsync(cancellationToken);
    }
}
