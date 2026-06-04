namespace SimpleCasePortal.Infrastructure.ExternalApis;

public sealed class ExternalApisOptions
{
    public bool UseFakeClientsInDevelopment { get; set; }

    public ExternalApiTargetOptions DentistApp { get; set; } = new();

    public ExternalApiTargetOptions WorkflowApp { get; set; } = new();

    public ExternalApiTargetOptions ProductionApp { get; set; } = new();
}
