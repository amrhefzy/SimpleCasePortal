using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Sync;

public sealed class ExternalCaseFileDto
{
    public FileTypeEnum FileType { get; set; }

    public string OriginalFileName { get; set; } = default!;

    public string ObjectKey { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public string? Checksum { get; set; }

    public string? DownloadUrl { get; set; }
}
