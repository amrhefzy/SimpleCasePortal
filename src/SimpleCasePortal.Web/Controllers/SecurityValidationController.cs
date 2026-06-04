using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Web.Authorization;

namespace SimpleCasePortal.Web.Controllers;

[Authorize]
public sealed class SecurityValidationController : Controller
{
    [Authorize(Policy = "Permission:Users.Manage")]
    public IActionResult UsersManage()
    {
        return Content($"Authorized for {PermissionNames.UsersManage}");
    }
}
