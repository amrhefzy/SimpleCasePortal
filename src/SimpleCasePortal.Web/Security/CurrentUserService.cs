using System.Security.Claims;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Application.Security;

namespace SimpleCasePortal.Web.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public string? UserId => FindClaim(ClaimTypes.NameIdentifier);

    public string? FullName => FindClaim(ApplicationClaimTypes.FullName);

    public string? Email => FindClaim(ClaimTypes.Email);

    public int? DoctorClinicId => int.TryParse(FindClaim(ApplicationClaimTypes.DoctorClinicId), out var id) ? id : null;

    public bool IsInRole(string roleName)
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(roleName) == true;
    }

    private string? FindClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }
}
