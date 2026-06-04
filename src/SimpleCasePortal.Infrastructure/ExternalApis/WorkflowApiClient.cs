using Microsoft.Extensions.Options;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Infrastructure.ExternalApis;

public sealed class WorkflowApiClient : ConfiguredExternalApiClient
{
    public WorkflowApiClient(IOptions<ExternalApisOptions> options)
        : base(options)
    {
    }

    public override SyncTargetEnum SyncTarget => SyncTargetEnum.WorkflowApp;

    protected override ExternalApiTargetOptions TargetOptions => GetTargetOptions();
}
