using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class SyncReportItemDto
{
    public SyncTargetEnum SyncTarget { get; set; }

    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public DateTime? LastSuccessDate { get; set; }

    public DateTime? LastFailureDate { get; set; }
}
