using SimpleCasePortal.Application.DTOs.Audit;

namespace SimpleCasePortal.Web.ViewModels.Audit;

public sealed class AuditLogDetailsViewModel
{
    public AuditLogDetailsDto Log { get; set; } = default!;
}
