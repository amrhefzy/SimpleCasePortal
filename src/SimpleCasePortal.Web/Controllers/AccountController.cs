using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleCasePortal.Application.DTOs.Auth;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Application.Security;
using SimpleCasePortal.Web.ViewModels.Account;

namespace SimpleCasePortal.Web.Controllers;

[AutoValidateAntiforgeryToken]
public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public AccountController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ValidateCredentialsAsync(
            new LoginRequestDto { Email = model.Email, Password = model.Password },
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        var claims = BuildClaims(result.Data);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true,
                IssuedUtc = DateTimeOffset.UtcNow
            });

        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _auditService.LogAsync(
            AuditActionNames.Logout,
            "ApplicationUser",
            userId ?? "unknown",
            userId,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent: Request.Headers.UserAgent.ToString(),
            cancellationToken: cancellationToken);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private static IEnumerable<Claim> BuildClaims(AuthenticatedUserDto user)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.Id);
        yield return new Claim(ClaimTypes.Name, user.FullName);
        yield return new Claim(ApplicationClaimTypes.FullName, user.FullName);
        yield return new Claim(ClaimTypes.Email, user.Email);

        if (user.DoctorClinicId.HasValue)
        {
            yield return new Claim(ApplicationClaimTypes.DoctorClinicId, user.DoctorClinicId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        foreach (var role in user.Roles)
        {
            yield return new Claim(ClaimTypes.Role, role);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
