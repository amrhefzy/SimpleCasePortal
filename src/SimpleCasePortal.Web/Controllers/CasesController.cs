using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleCasePortal.Application.DTOs.Cases;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Enums;
using SimpleCasePortal.Web.Authorization;
using SimpleCasePortal.Web.ViewModels.Cases;
using SimpleCasePortal.Web.ViewModels.Files;
using SimpleCasePortal.Web.ViewModels.Sync;

namespace SimpleCasePortal.Web.Controllers;

[Authorize]
public sealed class CasesController : Controller
{
    private readonly ICaseService _caseService;
    private readonly ICaseFileService _caseFileService;
    private readonly IExternalSyncService _externalSyncService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public CasesController(
        ICaseService caseService,
        ICaseFileService caseFileService,
        IExternalSyncService externalSyncService,
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _caseService = caseService;
        _caseFileService = caseFileService;
        _externalSyncService = externalSyncService;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CaseFilterViewModel filter, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var canViewAll = await HasPermissionAsync(PermissionNames.CasesViewAll, cancellationToken);
        var canViewOwn = await HasPermissionAsync(PermissionNames.CasesViewOwn, cancellationToken);

        if (!canViewAll && !canViewOwn)
        {
            return Forbid();
        }

        var response = await _caseService.GetCasesAsync(new CaseListFilterDto
        {
            CaseNumber = filter.CaseNumber,
            PatientName = filter.PatientName,
            Status = filter.Status,
            DoctorClinicId = filter.DoctorClinicId,
            CreatedFrom = filter.CreatedFrom,
            CreatedTo = filter.CreatedTo
        }, userId, cancellationToken);

        if (!response.Success)
        {
            return Forbid();
        }

        var doctorClinicOptions = canViewAll
            ? await BuildDoctorClinicOptionsAsync(userId, filter.DoctorClinicId, cancellationToken)
            : [];

        return View(new CaseListViewModel
        {
            Filter = filter,
            Cases = response.Data ?? [],
            DoctorClinicOptions = doctorClinicOptions,
            CanCreate = await HasPermissionAsync(PermissionNames.CasesCreate, cancellationToken),
            CanEdit = await HasPermissionAsync(PermissionNames.CasesUpdate, cancellationToken),
            CanDelete = await HasPermissionAsync(PermissionNames.CasesDeleteSoft, cancellationToken),
            CanFilterByDoctorClinic = canViewAll
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var response = await _caseService.GetCaseByIdAsync(id, RequireUserId(), cancellationToken);
        if (!response.Success || response.Data is null)
        {
            return Forbid();
        }

        var filesResponse = await _caseFileService.GetCaseFilesAsync(id, RequireUserId(), cancellationToken);
        var canUploadFiles = await HasPermissionAsync(PermissionNames.FilesUpload, cancellationToken);
        var canDownloadFiles = await HasPermissionAsync(PermissionNames.FilesDownload, cancellationToken);
        var canDeleteFiles = await HasPermissionAsync(PermissionNames.FilesDeleteSoft, cancellationToken);
        var syncStatusesResponse = await _externalSyncService.GetCaseSyncStatusAsync(id, cancellationToken);

        return View(new CaseDetailsViewModel
        {
            Case = response.Data,
            CanEdit = await HasPermissionAsync(PermissionNames.CasesUpdate, cancellationToken),
            CanDelete = await HasPermissionAsync(PermissionNames.CasesDeleteSoft, cancellationToken),
            CanUploadFiles = canUploadFiles,
            CanDownloadFiles = canDownloadFiles,
            CanDeleteFiles = canDeleteFiles,
            CanSyncDentist = await HasPermissionAsync(PermissionNames.SyncDentist, cancellationToken),
            CanSyncWorkflow = await HasPermissionAsync(PermissionNames.SyncWorkflow, cancellationToken),
            CanSyncProduction = await HasPermissionAsync(PermissionNames.SyncProduction, cancellationToken),
            CanRetrySync = await HasPermissionAsync(PermissionNames.SyncRetry, cancellationToken),
            Files = (filesResponse.Data ?? []).Select(file => new CaseFileViewModel
            {
                Id = file.Id,
                FileType = file.FileType,
                OriginalFileName = file.OriginalFileName,
                FileSizeBytes = file.FileSizeBytes,
                UploadedByUserId = file.UploadedByUserId,
                UploadedOn = file.UploadedOn,
                IsDeleted = file.IsDeleted
            }).ToArray(),
            SyncStatuses = (syncStatusesResponse.Data ?? []).Select(status => new CaseSyncStatusViewModel
            {
                SyncTarget = status.SyncTarget,
                SyncStatus = status.SyncStatus,
                LastSyncedOn = status.LastSyncedOn,
                ExternalReferenceId = status.ExternalReferenceId,
                LastErrorMessage = status.LastErrorMessage,
                LatestSyncLogId = status.LatestSyncLogId,
                CanRetry = status.CanRetry
            }).ToArray(),
            UploadFile = new UploadCaseFileViewModel { CaseId = id }
        });
    }

    [Authorize(Policy = "Permission:Cases.Create")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var canViewAll = await HasPermissionAsync(PermissionNames.CasesViewAll, cancellationToken);
        return View(new CreateCaseViewModel
        {
            CanSelectDoctorClinic = canViewAll,
            DoctorClinicOptions = canViewAll ? await BuildDoctorClinicOptionsAsync(RequireUserId(), null, cancellationToken) : []
        });
    }

    [Authorize(Policy = "Permission:Cases.Create")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCaseViewModel model, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var canViewAll = await HasPermissionAsync(PermissionNames.CasesViewAll, cancellationToken);

        if (!canViewAll)
        {
            ModelState.Remove(nameof(CreateCaseViewModel.DoctorClinicId));
        }

        ValidateCaseDates(model.DateOfBirth);
        if (!ModelState.IsValid)
        {
            await PopulateCreateModelAsync(model, canViewAll, cancellationToken);
            return View(model);
        }

        var response = await _caseService.CreateCaseAsync(new CreateCaseDto
        {
            DoctorClinicId = canViewAll ? model.DoctorClinicId : null,
            PatientName = model.PatientName,
            Age = model.Age,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Notes = model.Notes,
            CreatedByUserId = userId
        }, cancellationToken);

        if (!response.Success || response.Data is null)
        {
            AddModelErrors(response.Errors.Count > 0 ? response.Errors : [response.Message]);
            await PopulateCreateModelAsync(model, canViewAll, cancellationToken);
            return View(model);
        }

        TempData["StatusMessage"] = $"Case {response.Data.CaseNumber} created successfully.";
        return RedirectToAction(nameof(Details), new { id = response.Data.Id });
    }

    [Authorize(Policy = "Permission:Cases.Update")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var response = await _caseService.GetCaseByIdAsync(id, RequireUserId(), cancellationToken);
        if (!response.Success || response.Data is null)
        {
            return Forbid();
        }

        var canViewAll = await HasPermissionAsync(PermissionNames.CasesViewAll, cancellationToken);
        var model = new EditCaseViewModel
        {
            Id = response.Data.Id,
            CaseNumber = response.Data.CaseNumber,
            DoctorClinicId = response.Data.DoctorClinicId,
            PatientName = response.Data.PatientName,
            Age = response.Data.Age,
            DateOfBirth = response.Data.DateOfBirth,
            Gender = response.Data.Gender,
            Notes = response.Data.Notes,
            Status = response.Data.Status,
            CanSelectDoctorClinic = canViewAll,
            CanChangeStatus = canViewAll,
            DoctorClinicOptions = canViewAll ? await BuildDoctorClinicOptionsAsync(RequireUserId(), response.Data.DoctorClinicId, cancellationToken) : []
        };

        return View(model);
    }

    [Authorize(Policy = "Permission:Cases.Update")]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditCaseViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var canViewAll = await HasPermissionAsync(PermissionNames.CasesViewAll, cancellationToken);
        if (!canViewAll)
        {
            ModelState.Remove(nameof(EditCaseViewModel.DoctorClinicId));
            ModelState.Remove(nameof(EditCaseViewModel.Status));
        }

        ValidateCaseDates(model.DateOfBirth);
        if (!ModelState.IsValid)
        {
            await PopulateEditModelAsync(model, canViewAll, cancellationToken);
            return View(model);
        }

        var response = await _caseService.UpdateCaseAsync(new UpdateCaseDto
        {
            Id = model.Id,
            DoctorClinicId = canViewAll ? model.DoctorClinicId : null,
            PatientName = model.PatientName,
            Age = model.Age,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Notes = model.Notes,
            Status = canViewAll ? model.Status : null,
            UpdatedByUserId = RequireUserId()
        }, cancellationToken);

        if (!response.Success || response.Data is null)
        {
            AddModelErrors(response.Errors.Count > 0 ? response.Errors : [response.Message]);
            await PopulateEditModelAsync(model, canViewAll, cancellationToken);
            return View(model);
        }

        TempData["StatusMessage"] = $"Case {response.Data.CaseNumber} updated successfully.";
        return RedirectToAction(nameof(Details), new { id = response.Data.Id });
    }

    [Authorize(Policy = "Permission:Cases.Delete.Soft")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _caseService.SoftDeleteCaseAsync(id, RequireUserId(), cancellationToken);
        if (!response.Success)
        {
            return Forbid();
        }

        TempData["StatusMessage"] = "Case deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCreateModelAsync(CreateCaseViewModel model, bool canViewAll, CancellationToken cancellationToken)
    {
        model.CanSelectDoctorClinic = canViewAll;
        model.DoctorClinicOptions = canViewAll ? await BuildDoctorClinicOptionsAsync(RequireUserId(), model.DoctorClinicId, cancellationToken) : [];
    }

    private async Task PopulateEditModelAsync(EditCaseViewModel model, bool canViewAll, CancellationToken cancellationToken)
    {
        model.CanSelectDoctorClinic = canViewAll;
        model.CanChangeStatus = canViewAll;
        model.DoctorClinicOptions = canViewAll ? await BuildDoctorClinicOptionsAsync(RequireUserId(), model.DoctorClinicId, cancellationToken) : [];
    }

    private async Task<IReadOnlyCollection<SelectListItem>> BuildDoctorClinicOptionsAsync(
        string userId,
        int? selectedDoctorClinicId,
        CancellationToken cancellationToken)
    {
        var response = await _caseService.GetDoctorClinicOptionsAsync(userId, cancellationToken);
        return (response.Data ?? [])
            .Select(option => new SelectListItem(option.Name, option.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), option.Id == selectedDoctorClinicId))
            .ToArray();
    }

    private async Task<bool> HasPermissionAsync(string permissionName, CancellationToken cancellationToken)
    {
        return await _permissionService.HasPermissionAsync(RequireUserId(), permissionName, cancellationToken);
    }

    private string RequireUserId()
    {
        return _currentUserService.UserId ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }

    private void ValidateCaseDates(DateTime? dateOfBirth)
    {
        if (dateOfBirth.HasValue && dateOfBirth.Value.Date > DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(nameof(CreateCaseViewModel.DateOfBirth), "Date of birth cannot be in the future.");
        }
    }

    private void AddModelErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }
}
