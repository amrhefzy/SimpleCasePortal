using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Cases;

namespace SimpleCasePortal.Application.Interfaces;

public interface ICaseService
{
    Task<ApiResponse<IReadOnlyCollection<CaseDto>>> GetCasesAsync(
        CaseListFilterDto filter,
        string userId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CaseDto>> GetCaseByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CaseDto>> CreateCaseAsync(CreateCaseDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<CaseDto>> UpdateCaseAsync(UpdateCaseDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> SoftDeleteCaseAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<DoctorClinicOptionDto>>> GetDoctorClinicOptionsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
