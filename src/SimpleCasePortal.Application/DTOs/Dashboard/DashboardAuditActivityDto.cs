namespace SimpleCasePortal.Application.DTOs.Dashboard;

public sealed class DashboardAuditActivityDto
{
    public long Id { get; set; }

    public string Action { get; set; } = default!;

    public string EntityName { get; set; } = default!;

    public string EntityId { get; set; } = default!;

    public string UserDisplayName { get; set; } = "System";

    public DateTime CreatedOn { get; set; }
}
