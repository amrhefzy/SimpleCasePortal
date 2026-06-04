namespace SimpleCasePortal.Application.DTOs.Dashboard;

public sealed class DashboardSummaryDto
{
    public int TotalCases { get; set; }

    public int DraftCases { get; set; }

    public int SubmittedCases { get; set; }

    public int SyncedCases { get; set; }

    public int FailedSyncCases { get; set; }

    public int TotalUploadedFiles { get; set; }

    public bool CanViewCases { get; set; }

    public bool CanCreateCase { get; set; }

    public bool CanViewAudit { get; set; }

    public IReadOnlyCollection<DashboardCaseDto> RecentCases { get; set; } = [];

    public IReadOnlyCollection<DashboardSyncFailureDto> RecentSyncFailures { get; set; } = [];

    public IReadOnlyCollection<DashboardAuditActivityDto> RecentAuditActivities { get; set; } = [];
}
