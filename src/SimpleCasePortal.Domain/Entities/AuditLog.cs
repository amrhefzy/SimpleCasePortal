namespace SimpleCasePortal.Domain.Entities;

public sealed class AuditLog
{
    public long Id { get; set; }

    public string? UserId { get; set; }

    public string Action { get; set; } = default!;

    public string EntityName { get; set; } = default!;

    public string EntityId { get; set; } = default!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedOn { get; set; }
}
