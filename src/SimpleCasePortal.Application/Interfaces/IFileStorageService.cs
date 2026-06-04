using SimpleCasePortal.Application.DTOs.Files;

namespace SimpleCasePortal.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResultDto> UploadAsync(StorageUploadRequestDto request, CancellationToken cancellationToken = default);

    Task<SignedFileUrlDto> GenerateSignedDownloadUrlAsync(int fileId, string objectKey, string fileName, CancellationToken cancellationToken = default);

    Task<bool> ObjectExistsAsync(string objectKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);

    Task<(Stream Content, string ContentType, string FileName)> OpenSignedDownloadAsync(string token, CancellationToken cancellationToken = default);
}
