using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class FailedSyncReportItemDto
{
    public int SyncLogId { get; set; }

    public int CaseId { get; set; }

    public string CaseNumber { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public string DoctorClinicName { get; set; } = string.Empty;

    public SyncTargetEnum SyncTarget { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime LastFailedOn { get; set; }
}
