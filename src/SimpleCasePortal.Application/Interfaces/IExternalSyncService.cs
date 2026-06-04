using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Sync;

namespace SimpleCasePortal.Application.Interfaces;

public interface IExternalSyncService
{
    Task<ApiResponse<CaseSyncLogDto>> SyncCaseToDentistAsync(int caseId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CaseSyncLogDto>> SyncCaseToWorkflowAsync(int caseId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CaseSyncLogDto>> SyncCaseToProductionAsync(int caseId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CaseSyncLogDto>> RetrySyncAsync(int syncLogId, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<CaseSyncStatusDto>>> GetCaseSyncStatusAsync(int caseId, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<CaseSyncLogDto>>> GetCaseSyncLogsAsync(int caseId, CancellationToken cancellationToken = default);
}
