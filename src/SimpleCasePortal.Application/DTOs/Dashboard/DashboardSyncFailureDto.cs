using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Dashboard;

public sealed class DashboardSyncFailureDto
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public string CaseNumber { get; set; } = default!;

    public SyncTargetEnum SyncTarget { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime SyncedOn { get; set; }
}
