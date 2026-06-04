using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleCasePortal.Application.DTOs.Audit;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Web.ViewModels.Audit;

namespace SimpleCasePortal.Web.Controllers;

[Authorize(Policy = "Permission:Audit.View")]
public sealed class AuditController : Controller
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(AuditLogFilterViewModel filter, CancellationToken cancellationToken)
    {
        var filterDto = new AuditLogFilterDto
        {
            Action = filter.Action,
            EntityName = filter.EntityName,
            UserId = filter.UserId,
            EntityId = filter.EntityId,
            SearchText = filter.SearchText,
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        var logsResponse = await _auditLogService.GetAuditLogsAsync(filterDto, cancellationToken);
        if (!logsResponse.Success || logsResponse.Data is null)
        {
            return Forbid();
        }

        return View(new AuditLogListViewModel
        {
            Filter = new AuditLogFilterViewModel
            {
                Action = filterDto.Action,
                EntityName = filterDto.EntityName,
                UserId = filterDto.UserId,
                EntityId = filterDto.EntityId,
                SearchText = filterDto.SearchText,
                DateFrom = filterDto.DateFrom,
                DateTo = filterDto.DateTo,
                PageNumber = logsResponse.Data.PageNumber,
                PageSize = logsResponse.Data.PageSize
            },
            Logs = logsResponse.Data.Logs,
            TotalCount = logsResponse.Data.TotalCount,
            PageNumber = logsResponse.Data.PageNumber,
            PageSize = logsResponse.Data.PageSize,
            TotalPages = logsResponse.Data.TotalPages,
            ActionOptions = await BuildOptionsAsync(_auditLogService.GetAuditActionOptionsAsync, filterDto.Action, cancellationToken),
            EntityNameOptions = await BuildOptionsAsync(_auditLogService.GetEntityNameOptionsAsync, filterDto.EntityName, cancellationToken),
            UserOptions = await BuildOptionsAsync(_auditLogService.GetUserOptionsAsync, filterDto.UserId, cancellationToken)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var response = await _auditLogService.GetAuditLogByIdAsync(id, cancellationToken);
        if (!response.Success || response.Data is null)
        {
            return NotFound();
        }

        return View(new AuditLogDetailsViewModel { Log = response.Data });
    }

    private static async Task<IReadOnlyCollection<SelectListItem>> BuildOptionsAsync(
        Func<CancellationToken, Task<SimpleCasePortal.Application.Common.ApiResponse<IReadOnlyCollection<AuditOptionDto>>>> loader,
        string? selectedValue,
        CancellationToken cancellationToken)
    {
        var response = await loader(cancellationToken);
        return (response.Data ?? [])
            .Select(option => new SelectListItem(option.Text, option.Value, option.Value == selectedValue))
            .ToArray();
    }
}
