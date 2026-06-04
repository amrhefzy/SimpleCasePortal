using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Domain.Entities;

public sealed class CaseSyncLog
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public SyncTargetEnum SyncTarget { get; set; }

    public SyncStatusEnum SyncStatus { get; set; } = SyncStatusEnum.Pending;

    public string RequestPayload { get; set; } = default!;

    public string? ResponsePayload { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ExternalReferenceId { get; set; }

    public string SyncedByUserId { get; set; } = default!;

    public DateTime SyncedOn { get; set; }

    public int RetryCount { get; set; }

    public Case Case { get; set; } = default!;
}
