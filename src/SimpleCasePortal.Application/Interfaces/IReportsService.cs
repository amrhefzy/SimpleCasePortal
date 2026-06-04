using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Reports;

namespace SimpleCasePortal.Application.Interfaces;

public interface IReportsService
{
    Task<ApiResponse<CaseSummaryReportDto>> GetCaseSummaryReportAsync(string userId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<CaseStatusReportItemDto>>> GetCaseStatusReportAsync(string userId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<DoctorClinicActivityReportItemDto>>> GetDoctorClinicActivityReportAsync(string userId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<FileUploadReportDto>> GetFileUploadReportAsync(string userId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<SyncReportItemDto>>> GetSyncReportAsync(string userId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<FailedSyncReportItemDto>>> GetFailedSyncReportAsync(string userId, ReportFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<DoctorClinicReportOptionDto>>> GetDoctorClinicOptionsAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> CanFilterByDoctorClinicAsync(string userId, CancellationToken cancellationToken = default);
}
