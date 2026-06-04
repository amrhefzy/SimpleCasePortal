using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using SimpleCasePortal.Application.DTOs.Files;
using SimpleCasePortal.Application.Interfaces;

namespace SimpleCasePortal.Infrastructure.Storage;

public sealed class LocalDevelopmentStorageService : IFileStorageService
{
    private readonly IDataProtector _protector;
    private readonly StorageOptions _options;
    private readonly string _rootPath;

    public LocalDevelopmentStorageService(IDataProtectionProvider dataProtectionProvider, IOptions<StorageOptions> options)
    {
        _protector = dataProtectionProvider.CreateProtector("SimpleCasePortal.LocalSignedFiles.v1");
        _options = options.Value;
        _rootPath = Path.GetFullPath(_options.LocalRootPath ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "LocalStorage"));
    }

    public async Task<FileUploadResultDto> UploadAsync(StorageUploadRequestDto request, CancellationToken cancellationToken = default)
    {
        var destinationPath = GetSafePath(request.ObjectKey);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        await using var fileStream = File.Create(destinationPath);
        await request.Content.CopyToAsync(fileStream, cancellationToken);

        return new FileUploadResultDto
        {
            ObjectKey = request.ObjectKey,
            StoredFileName = request.StoredFileName,
            FileSizeBytes = request.FileSizeBytes,
            ContentType = request.ContentType,
            Checksum = request.Checksum
        };
    }

    public Task<SignedFileUrlDto> GenerateSignedDownloadUrlAsync(int fileId, string objectKey, string fileName, CancellationToken cancellationToken = default)
    {
        var expiresOnUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, _options.SignedUrlExpiryMinutes));
        var payload = string.Join("|", objectKey, fileName, expiresOnUtc.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var token = Uri.EscapeDataString(_protector.Protect(payload));
        var baseUrl = (_options.LocalBaseUrl ?? string.Empty).TrimEnd('/');

        return Task.FromResult(new SignedFileUrlDto
        {
            FileId = fileId,
            FileName = fileName,
            Url = $"{baseUrl}/CaseFiles/LocalSignedDownload?token={token}",
            ExpiresOnUtc = expiresOnUtc
        });
    }

    public Task<bool> ObjectExistsAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(GetSafePath(objectKey)));
    }

    public Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<(Stream Content, string ContentType, string FileName)> OpenSignedDownloadAsync(string token, CancellationToken cancellationToken = default)
    {
        var payload = _protector.Unprotect(token);
        var parts = payload.Split('|', 3);
        if (parts.Length != 3 || !long.TryParse(parts[2], out var ticks))
        {
            throw new InvalidOperationException("Invalid signed file token.");
        }

        var expiresOnUtc = new DateTime(ticks, DateTimeKind.Utc);
        if (DateTime.UtcNow > expiresOnUtc)
        {
            throw new InvalidOperationException("Signed file token has expired.");
        }

        var path = GetSafePath(parts[0]);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File was not found.", parts[1]);
        }

        var contentType = GetContentType(Path.GetExtension(parts[1]));
        return Task.FromResult<(Stream Content, string ContentType, string FileName)>((File.OpenRead(path), contentType, parts[1]));
    }

    private string GetSafePath(string objectKey)
    {
        var relativePath = objectKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid object key.");
        }

        return fullPath;
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".stl" => "application/octet-stream",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
