using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Audit;

namespace SimpleCasePortal.Application.Interfaces;

public interface IAuditLogService
{
    Task<ApiResponse<AuditLogListDto>> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<AuditLogDetailsDto>> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<AuditOptionDto>>> GetAuditActionOptionsAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<AuditOptionDto>>> GetEntityNameOptionsAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<AuditOptionDto>>> GetUserOptionsAsync(CancellationToken cancellationToken = default);
}
