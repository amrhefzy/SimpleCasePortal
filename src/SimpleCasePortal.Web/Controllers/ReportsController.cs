using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleCasePortal.Application.DTOs.Reports;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Enums;
using SimpleCasePortal.Web.ViewModels.Reports;

namespace SimpleCasePortal.Web.Controllers;

[Authorize(Policy = "Permission:Reports.View")]
public sealed class ReportsController : Controller
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IReportsService _reportsService;

    public ReportsController(ICurrentUserService currentUserService, IReportsService reportsService)
    {
        _currentUserService = currentUserService;
        _reportsService = reportsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ReportFilterViewModel filter, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var dto = ToDto(filter);

        var caseSummary = await _reportsService.GetCaseSummaryReportAsync(userId, dto, cancellationToken);
        if (!caseSummary.Success)
        {
            return Forbid();
        }

        var canFilterByDoctorClinic = await _reportsService.CanFilterByDoctorClinicAsync(userId, cancellationToken);
        if (!canFilterByDoctorClinic)
        {
            filter.DoctorClinicId = null;
        }

        var doctorClinicOptionsResponse = await _reportsService.GetDoctorClinicOptionsAsync(userId, cancellationToken);
        var caseStatus = await _reportsService.GetCaseStatusReportAsync(userId, dto, cancellationToken);
        var activity = await _reportsService.GetDoctorClinicActivityReportAsync(userId, dto, cancellationToken);
        var files = await _reportsService.GetFileUploadReportAsync(userId, dto, cancellationToken);
        var sync = await _reportsService.GetSyncReportAsync(userId, dto, cancellationToken);
        var failedSync = await _reportsService.GetFailedSyncReportAsync(userId, dto, cancellationToken);

        return View(new ReportsDashboardViewModel
        {
            Filter = filter,
            CanFilterByDoctorClinic = canFilterByDoctorClinic,
            DoctorClinicOptions = BuildDoctorClinicOptions(doctorClinicOptionsResponse.Data ?? [], filter.DoctorClinicId),
            CaseStatusOptions = BuildEnumOptions<CaseStatusEnum>(filter.CaseStatus),
            SyncTargetOptions = BuildEnumOptions<SyncTargetEnum>(filter.SyncTarget),
            SyncStatusOptions = BuildEnumOptions<SyncStatusEnum>(filter.SyncStatus),
            CaseSummary = caseSummary.Data ?? new(),
            CaseStatusDistribution = caseStatus.Data ?? [],
            DoctorClinicActivity = activity.Data ?? [],
            FileUploadReport = files.Data ?? new(),
            SyncReport = sync.Data ?? [],
            FailedSyncs = failedSync.Data ?? []
        });
    }

    private static ReportFilterDto ToDto(ReportFilterViewModel filter)
    {
        return new ReportFilterDto
        {
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo,
            DoctorClinicId = filter.DoctorClinicId,
            CaseStatus = filter.CaseStatus,
            SyncTarget = filter.SyncTarget,
            SyncStatus = filter.SyncStatus,
            SearchText = filter.SearchText
        };
    }

    private static IReadOnlyCollection<SelectListItem> BuildDoctorClinicOptions(
        IReadOnlyCollection<DoctorClinicReportOptionDto> options,
        int? selectedId)
    {
        return options
            .Select(option => new SelectListItem(option.Name, option.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), option.Id == selectedId))
            .ToArray();
    }

    private static IReadOnlyCollection<SelectListItem> BuildEnumOptions<TEnum>(TEnum? selected)
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(value => new SelectListItem(value.ToString(), Convert.ToInt32(value).ToString(System.Globalization.CultureInfo.InvariantCulture), selected.HasValue && EqualityComparer<TEnum>.Default.Equals(selected.Value, value)))
            .ToArray();
    }

    private string RequireUserId()
    {
        return _currentUserService.UserId ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
