namespace SimpleCasePortal.Application.DTOs.Files;

public sealed class FileUploadResultDto
{
    public string ObjectKey { get; set; } = default!;

    public string StoredFileName { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public string ContentType { get; set; } = default!;

    public string? Checksum { get; set; }
}
