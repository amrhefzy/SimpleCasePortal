using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Dashboard;

namespace SimpleCasePortal.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetDashboardAsync(string userId, CancellationToken cancellationToken = default);
}
