namespace SimpleCasePortal.Infrastructure.ExternalApis;

public sealed class ExternalApiTargetOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "/api/cases";

    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;

    public bool SendSignedFileUrls { get; set; }
}
