using SimpleCasePortal.Application.DTOs.Dashboard;

namespace SimpleCasePortal.Web.ViewModels.Dashboard;

public sealed class DashboardViewModel
{
    public DashboardSummaryDto Summary { get; set; } = new();
}
