using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class ReportFilterDto
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public int? DoctorClinicId { get; set; }

    public CaseStatusEnum? CaseStatus { get; set; }

    public SyncTargetEnum? SyncTarget { get; set; }

    public SyncStatusEnum? SyncStatus { get; set; }

    public string? SearchText { get; set; }
}
