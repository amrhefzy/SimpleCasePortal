namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class FileUploadReportDto
{
    public int TotalFiles { get; set; }

    public long TotalFileSizeBytes { get; set; }

    public int StlFileCount { get; set; }

    public IReadOnlyCollection<FileTypeReportItemDto> FilesByType { get; set; } = [];

    public IReadOnlyCollection<FileUploadDateReportItemDto> UploadsByDate { get; set; } = [];
}
