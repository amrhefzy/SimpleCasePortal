namespace SimpleCasePortal.Application.DTOs.Files;

public sealed class StorageUploadRequestDto
{
    public Stream Content { get; set; } = default!;

    public string ObjectKey { get; set; } = default!;

    public string StoredFileName { get; set; } = default!;

    public string ContentType { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public string? Checksum { get; set; }
}
