namespace SimpleCasePortal.Application.DTOs.Audit;

public sealed class AuditLogDto
{
    public long Id { get; set; }

    public string? UserId { get; set; }

    public string UserDisplayName { get; set; } = "System";

    public string Action { get; set; } = default!;

    public string EntityName { get; set; } = default!;

    public string EntityId { get; set; } = default!;

    public string? IpAddress { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}
