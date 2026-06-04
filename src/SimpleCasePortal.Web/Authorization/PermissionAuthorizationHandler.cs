using Microsoft.AspNetCore.Authorization;
using SimpleCasePortal.Application.Interfaces;

namespace SimpleCasePortal.Web.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (await _permissionService.HasPermissionAsync(userId, requirement.PermissionName))
        {
            context.Succeed(requirement);
        }
    }
}
