namespace SimpleCasePortal.Application.DTOs.Audit;

public sealed class AuditLogFilterDto
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;
    private int _pageSize = 25;

    public string? Action { get; set; }

    public string? EntityName { get; set; }

    public string? UserId { get; set; }

    public string? EntityId { get; set; }

    public string? SearchText { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }
}
