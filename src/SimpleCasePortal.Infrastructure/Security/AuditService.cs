using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Entities;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Security;

public sealed class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;

    public AuditService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        string entityId,
        string? userId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedOn = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
