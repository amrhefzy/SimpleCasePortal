using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Sync;

public sealed class CaseSyncStatusDto
{
    public SyncTargetEnum SyncTarget { get; set; }

    public SyncStatusEnum? SyncStatus { get; set; }

    public DateTime? LastSyncedOn { get; set; }

    public string? ExternalReferenceId { get; set; }

    public string? LastErrorMessage { get; set; }

    public int? LatestSyncLogId { get; set; }

    public bool CanRetry { get; set; }
}
