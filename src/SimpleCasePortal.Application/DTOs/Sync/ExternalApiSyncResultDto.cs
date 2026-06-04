namespace SimpleCasePortal.Application.DTOs.Sync;

public sealed class ExternalApiSyncResultDto
{
    public bool Success { get; set; }

    public string? ExternalReferenceId { get; set; }

    public string Message { get; set; } = default!;

    public string? ErrorCode { get; set; }

    public int? StatusCode { get; set; }

    public string? ResponsePayload { get; set; }
}
