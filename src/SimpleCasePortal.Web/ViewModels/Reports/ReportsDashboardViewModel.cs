using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleCasePortal.Application.DTOs.Reports;

namespace SimpleCasePortal.Web.ViewModels.Reports;

public sealed class ReportsDashboardViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();

    public bool CanFilterByDoctorClinic { get; set; }

    public IReadOnlyCollection<SelectListItem> DoctorClinicOptions { get; set; } = [];

    public IReadOnlyCollection<SelectListItem> CaseStatusOptions { get; set; } = [];

    public IReadOnlyCollection<SelectListItem> SyncTargetOptions { get; set; } = [];

    public IReadOnlyCollection<SelectListItem> SyncStatusOptions { get; set; } = [];

    public CaseSummaryReportDto CaseSummary { get; set; } = new();

    public IReadOnlyCollection<CaseStatusReportItemDto> CaseStatusDistribution { get; set; } = [];

    public IReadOnlyCollection<DoctorClinicActivityReportItemDto> DoctorClinicActivity { get; set; } = [];

    public FileUploadReportDto FileUploadReport { get; set; } = new();

    public IReadOnlyCollection<SyncReportItemDto> SyncReport { get; set; } = [];

    public IReadOnlyCollection<FailedSyncReportItemDto> FailedSyncs { get; set; } = [];
}
