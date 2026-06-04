using Microsoft.Extensions.Options;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Infrastructure.ExternalApis;

public sealed class ProductionApiClient : ConfiguredExternalApiClient
{
    public ProductionApiClient(IOptions<ExternalApisOptions> options)
        : base(options)
    {
    }

    public override SyncTargetEnum SyncTarget => SyncTargetEnum.ProductionApp;

    protected override ExternalApiTargetOptions TargetOptions => GetTargetOptions();
}
