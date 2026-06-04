using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Sync;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;

namespace SimpleCasePortal.Web.Controllers;

[Authorize]
public sealed class SyncController : Controller
{
    private readonly IExternalSyncService _externalSyncService;

    public SyncController(IExternalSyncService externalSyncService)
    {
        _externalSyncService = externalSyncService;
    }

    [Authorize(Policy = "Permission:Sync.Dentist")]
    [HttpPost("Sync/Dentist/{caseId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncToDentist(int caseId, CancellationToken cancellationToken)
    {
        var response = await _externalSyncService.SyncCaseToDentistAsync(caseId, cancellationToken);
        SetTempData(response);
        return RedirectToCaseDetails(caseId);
    }

    [Authorize(Policy = "Permission:Sync.Workflow")]
    [HttpPost("Sync/Workflow/{caseId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncToWorkflow(int caseId, CancellationToken cancellationToken)
    {
        var response = await _externalSyncService.SyncCaseToWorkflowAsync(caseId, cancellationToken);
        SetTempData(response);
        return RedirectToCaseDetails(caseId);
    }

    [Authorize(Policy = "Permission:Sync.Production")]
    [HttpPost("Sync/Production/{caseId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncToProduction(int caseId, CancellationToken cancellationToken)
    {
        var response = await _externalSyncService.SyncCaseToProductionAsync(caseId, cancellationToken);
        SetTempData(response);
        return RedirectToCaseDetails(caseId);
    }

    [Authorize(Policy = "Permission:Sync.Retry")]
    [HttpPost("Sync/Retry/{syncLogId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retry(int syncLogId, int caseId, CancellationToken cancellationToken)
    {
        var response = await _externalSyncService.RetrySyncAsync(syncLogId, cancellationToken);
        SetTempData(response);
        return RedirectToCaseDetails(caseId);
    }

    private void SetTempData(ApiResponse<CaseSyncLogDto> response)
    {
        if (response.Success)
        {
            TempData["StatusMessage"] = response.Message;
            return;
        }

        TempData["SyncError"] = response.Errors.Count > 0
            ? string.Join(" ", response.Errors)
            : response.Message;
    }

    private RedirectToActionResult RedirectToCaseDetails(int caseId)
    {
        return RedirectToAction("Details", "Cases", new { id = caseId });
    }
}
