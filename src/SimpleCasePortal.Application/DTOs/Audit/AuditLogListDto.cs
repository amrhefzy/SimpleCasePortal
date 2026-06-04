namespace SimpleCasePortal.Application.DTOs.Audit;

public sealed class AuditLogListDto
{
    public IReadOnlyCollection<AuditLogDto> Logs { get; set; } = [];

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
