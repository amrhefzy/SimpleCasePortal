namespace SimpleCasePortal.Application.DTOs.Audit;

public sealed class AuditLogDetailsDto
{
    public long Id { get; set; }

    public string? UserId { get; set; }

    public string UserDisplayName { get; set; } = "System";

    public string Action { get; set; } = default!;

    public string EntityName { get; set; } = default!;

    public string EntityId { get; set; } = default!;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime CreatedOn { get; set; }
}
