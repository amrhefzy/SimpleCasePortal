using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Web.ViewModels.Files;

public sealed class CaseFileViewModel
{
    public int Id { get; set; }

    public FileTypeEnum FileType { get; set; }

    public string OriginalFileName { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public string UploadedByUserId { get; set; } = default!;

    public DateTime UploadedOn { get; set; }

    public bool IsDeleted { get; set; }
}
