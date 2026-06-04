using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Web.Models;
using SimpleCasePortal.Web.ViewModels.Dashboard;

namespace SimpleCasePortal.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDashboardService _dashboardService;

    public HomeController(ICurrentUserService currentUserService, IDashboardService dashboardService)
    {
        _currentUserService = currentUserService;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new InvalidOperationException("Authenticated user id is missing.");
        var response = await _dashboardService.GetDashboardAsync(userId, cancellationToken);

        return View(new DashboardViewModel
        {
            Summary = response.Data ?? new()
        });
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode
        });
    }
}
