using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class FileTypeReportItemDto
{
    public FileTypeEnum FileType { get; set; }

    public int Count { get; set; }

    public long TotalFileSizeBytes { get; set; }
}
