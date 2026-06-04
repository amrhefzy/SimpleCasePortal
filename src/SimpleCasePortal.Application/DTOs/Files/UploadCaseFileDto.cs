using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Files;

public sealed class UploadCaseFileDto
{
    public FileTypeEnum FileType { get; set; }

    public string OriginalFileName { get; set; } = default!;

    public string ContentType { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public Stream Content { get; set; } = default!;

    public string UploadedByUserId { get; set; } = default!;
}
