using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Files;

public sealed class CaseFileDto
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public FileTypeEnum FileType { get; set; }

    public string OriginalFileName { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public string ContentType { get; set; } = default!;

    public string FileExtension { get; set; } = default!;

    public string? Checksum { get; set; }

    public string UploadedByUserId { get; set; } = default!;

    public DateTime UploadedOn { get; set; }

    public bool IsDeleted { get; set; }
}
