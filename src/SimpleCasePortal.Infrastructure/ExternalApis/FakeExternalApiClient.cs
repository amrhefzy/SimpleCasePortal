using System.Text.Json;
using SimpleCasePortal.Application.DTOs.Sync;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Infrastructure.ExternalApis;

public sealed class FakeExternalApiClient : IExternalApiClient
{
    public FakeExternalApiClient(SyncTargetEnum syncTarget)
    {
        SyncTarget = syncTarget;
    }

    public SyncTargetEnum SyncTarget { get; }

    public Task<ExternalApiSyncResultDto> SendCaseAsync(
        ExternalCaseSyncPayloadDto payload,
        CancellationToken cancellationToken = default)
    {
        if (payload.CaseNumber.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ExternalApiSyncResultDto
            {
                Success = false,
                ErrorCode = "DEV_FAKE_FAILURE",
                Message = "Development fake sync failed because the case number contains FAIL.",
                StatusCode = 422,
                ResponsePayload = JsonSerializer.Serialize(new
                {
                    success = false,
                    errorCode = "DEV_FAKE_FAILURE",
                    message = "Development fake sync failed"
                })
            });
        }

        var prefix = SyncTarget switch
        {
            SyncTargetEnum.DentistApp => "DEV-DENT",
            SyncTargetEnum.WorkflowApp => "DEV-WF",
            SyncTargetEnum.ProductionApp => "DEV-PROD",
            _ => "DEV"
        };

        var externalReferenceId = $"{prefix}-{payload.CaseNumber}";
        return Task.FromResult(new ExternalApiSyncResultDto
        {
            Success = true,
            ExternalReferenceId = externalReferenceId,
            Message = "Development fake sync succeeded.",
            StatusCode = 200,
            ResponsePayload = JsonSerializer.Serialize(new
            {
                success = true,
                externalReferenceId,
                message = "Development fake sync succeeded"
            })
        });
    }
}
