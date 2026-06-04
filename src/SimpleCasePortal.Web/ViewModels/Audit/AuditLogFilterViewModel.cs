using System.ComponentModel.DataAnnotations;

namespace SimpleCasePortal.Web.ViewModels.Audit;

public sealed class AuditLogFilterViewModel
{
    public string? Action { get; set; }

    [Display(Name = "Entity")]
    public string? EntityName { get; set; }

    [Display(Name = "User")]
    public string? UserId { get; set; }

    [Display(Name = "Entity ID")]
    public string? EntityId { get; set; }

    [Display(Name = "Search")]
    public string? SearchText { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date from")]
    public DateTime? DateFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date to")]
    public DateTime? DateTo { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 25;
}
