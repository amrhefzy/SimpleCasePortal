namespace SimpleCasePortal.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityName,
        string entityId,
        string? userId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}
