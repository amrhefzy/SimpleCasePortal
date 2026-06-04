namespace SimpleCasePortal.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permissionName, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default);
}
