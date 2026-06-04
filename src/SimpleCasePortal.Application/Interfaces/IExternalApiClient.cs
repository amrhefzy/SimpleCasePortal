using SimpleCasePortal.Application.DTOs.Sync;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.Interfaces;

public interface IExternalApiClient
{
    SyncTargetEnum SyncTarget { get; }

    Task<ExternalApiSyncResultDto> SendCaseAsync(
        ExternalCaseSyncPayloadDto payload,
        CancellationToken cancellationToken = default);
}
