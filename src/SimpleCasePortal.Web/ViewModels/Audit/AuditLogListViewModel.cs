using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleCasePortal.Application.DTOs.Audit;

namespace SimpleCasePortal.Web.ViewModels.Audit;

public sealed class AuditLogListViewModel
{
    public AuditLogFilterViewModel Filter { get; set; } = new();

    public IReadOnlyCollection<AuditLogDto> Logs { get; set; } = [];

    public IReadOnlyCollection<SelectListItem> ActionOptions { get; set; } = [];

    public IReadOnlyCollection<SelectListItem> EntityNameOptions { get; set; } = [];

    public IReadOnlyCollection<SelectListItem> UserOptions { get; set; } = [];

    public int TotalCount { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public int TotalPages { get; set; } = 1;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
