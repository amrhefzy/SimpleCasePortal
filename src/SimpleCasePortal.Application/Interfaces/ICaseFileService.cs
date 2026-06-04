using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Files;

namespace SimpleCasePortal.Application.Interfaces;

public interface ICaseFileService
{
    Task<ApiResponse<IReadOnlyCollection<CaseFileDto>>> GetCaseFilesAsync(int caseId, string userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CaseFileDto>> UploadCaseFileAsync(int caseId, UploadCaseFileDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<SignedFileUrlDto>> GetSignedDownloadUrlAsync(int fileId, string userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> SoftDeleteCaseFileAsync(int fileId, string userId, CancellationToken cancellationToken = default);
}
