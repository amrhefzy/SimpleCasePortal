using Microsoft.Extensions.Options;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Infrastructure.ExternalApis;

public sealed class DentistApiClient : ConfiguredExternalApiClient
{
    public DentistApiClient(IOptions<ExternalApisOptions> options)
        : base(options)
    {
    }

    public override SyncTargetEnum SyncTarget => SyncTargetEnum.DentistApp;

    protected override ExternalApiTargetOptions TargetOptions => GetTargetOptions();
}
