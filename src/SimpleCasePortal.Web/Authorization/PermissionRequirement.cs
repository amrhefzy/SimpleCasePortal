using Microsoft.AspNetCore.Authorization;

namespace SimpleCasePortal.Web.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionName)
    {
        PermissionName = permissionName;
    }

    public string PermissionName { get; }
}
