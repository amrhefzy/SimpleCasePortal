using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Domain.Entities;

public sealed class CaseFile
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public FileTypeEnum FileType { get; set; }

    public string OriginalFileName { get; set; } = default!;

    public string StoredFileName { get; set; } = default!;

    public string ObjectKey { get; set; } = default!;

    public string ContentType { get; set; } = default!;

    public string FileExtension { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public string? Checksum { get; set; }

    public string UploadedByUserId { get; set; } = default!;

    public DateTime UploadedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public string? DeletedByUserId { get; set; }

    public Case Case { get; set; } = default!;
}
