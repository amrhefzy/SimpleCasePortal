using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Sync;

public sealed class CaseSyncLogDto
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public SyncTargetEnum SyncTarget { get; set; }

    public SyncStatusEnum SyncStatus { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ExternalReferenceId { get; set; }

    public string SyncedByUserId { get; set; } = default!;

    public DateTime SyncedOn { get; set; }

    public int RetryCount { get; set; }
}
