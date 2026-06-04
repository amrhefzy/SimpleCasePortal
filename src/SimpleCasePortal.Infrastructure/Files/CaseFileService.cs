using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Files;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Entities;
using SimpleCasePortal.Domain.Enums;
using SimpleCasePortal.Infrastructure.Data;
using SimpleCasePortal.Infrastructure.Storage;

namespace SimpleCasePortal.Infrastructure.Files;

public sealed class CaseFileService : ICaseFileService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".stl",
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf",
        ".zip"
    };

    private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe",
        ".js",
        ".html",
        ".htm",
        ".bat",
        ".cmd",
        ".ps1",
        ".php"
    };

    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICaseAuthorizationService _caseAuthorizationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPermissionService _permissionService;
    private readonly StorageOptions _storageOptions;

    public CaseFileService(
        AppDbContext dbContext,
        IAuditService auditService,
        ICaseAuthorizationService caseAuthorizationService,
        IFileStorageService fileStorageService,
        IPermissionService permissionService,
        IOptions<StorageOptions> storageOptions)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _caseAuthorizationService = caseAuthorizationService;
        _fileStorageService = fileStorageService;
        _permissionService = permissionService;
        _storageOptions = storageOptions.Value;
    }

    public async Task<ApiResponse<IReadOnlyCollection<CaseFileDto>>> GetCaseFilesAsync(int caseId, string userId, CancellationToken cancellationToken = default)
    {
        if (!await CanUseFilePermissionAsync(userId, PermissionNames.FilesView, caseId, cancellationToken))
        {
            return ApiResponse<IReadOnlyCollection<CaseFileDto>>.Fail("You do not have permission to view files for this case.");
        }

        var files = await _dbContext.CaseFiles
            .AsNoTracking()
            .Where(file => file.CaseId == caseId && !file.IsDeleted)
            .OrderByDescending(file => file.UploadedOn)
            .Select(file => ToDto(file))
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CaseFileDto>>.Ok(files);
    }

    public async Task<ApiResponse<CaseFileDto>> UploadCaseFileAsync(int caseId, UploadCaseFileDto dto, CancellationToken cancellationToken = default)
    {
        if (!await CanUseFilePermissionAsync(dto.UploadedByUserId, PermissionNames.FilesUpload, caseId, cancellationToken))
        {
            return ApiResponse<CaseFileDto>.Fail("You do not have permission to upload files for this case.");
        }

        var caseEntity = await _dbContext.Cases
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == caseId && !entity.IsDeleted, cancellationToken);

        if (caseEntity is null)
        {
            return ApiResponse<CaseFileDto>.Fail("Case was not found.");
        }

        var validationErrors = ValidateFile(dto);
        if (validationErrors.Count > 0)
        {
            await _auditService.LogAsync(
                "CaseFile.UploadFailed",
                "Case",
                caseId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                dto.UploadedByUserId,
                newValues: JsonSerializer.Serialize(new { dto.OriginalFileName, dto.FileType, validationErrors }),
                cancellationToken: cancellationToken);

            return ApiResponse<CaseFileDto>.Fail("File upload failed validation.", validationErrors);
        }

        await using var bufferedStream = new MemoryStream();
        await dto.Content.CopyToAsync(bufferedStream, cancellationToken);
        bufferedStream.Position = 0;
        var checksum = Convert.ToHexString(await SHA256.HashDataAsync(bufferedStream, cancellationToken)).ToLowerInvariant();
        bufferedStream.Position = 0;

        var extension = Path.GetExtension(dto.OriginalFileName).ToLowerInvariant();
        var safeFileName = CreateSafeFileName(Path.GetFileNameWithoutExtension(dto.OriginalFileName), extension);
        var uniqueFileId = Guid.NewGuid().ToString("N")[..12];
        var storedFileName = $"{uniqueFileId}_{safeFileName}";
        var now = DateTime.UtcNow;
        var objectKey = $"cases/{caseEntity.CaseNumber}/{dto.FileType}/{now:yyyy}/{now:MM}/{storedFileName}";

        var uploadResult = await _fileStorageService.UploadAsync(new StorageUploadRequestDto
        {
            Content = bufferedStream,
            ObjectKey = objectKey,
            StoredFileName = storedFileName,
            ContentType = NormalizeContentType(dto.ContentType, extension),
            FileSizeBytes = dto.FileSizeBytes,
            Checksum = checksum
        }, cancellationToken);

        var caseFile = new CaseFile
        {
            CaseId = caseId,
            FileType = dto.FileType,
            OriginalFileName = Path.GetFileName(dto.OriginalFileName),
            StoredFileName = uploadResult.StoredFileName,
            ObjectKey = uploadResult.ObjectKey,
            ContentType = uploadResult.ContentType,
            FileExtension = extension,
            FileSizeBytes = uploadResult.FileSizeBytes,
            Checksum = uploadResult.Checksum,
            UploadedByUserId = dto.UploadedByUserId,
            UploadedOn = now
        };

        await _dbContext.CaseFiles.AddAsync(caseFile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "CaseFile.Uploaded",
            "CaseFile",
            caseFile.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            dto.UploadedByUserId,
            newValues: JsonSerializer.Serialize(new
            {
                CaseId = caseId,
                caseEntity.CaseNumber,
                caseFile.OriginalFileName,
                caseFile.FileType,
                caseFile.Checksum
            }),
            cancellationToken: cancellationToken);

        return ApiResponse<CaseFileDto>.Ok(ToDto(caseFile), "File uploaded successfully.");
    }

    public async Task<ApiResponse<SignedFileUrlDto>> GetSignedDownloadUrlAsync(int fileId, string userId, CancellationToken cancellationToken = default)
    {
        var file = await _dbContext.CaseFiles
            .AsNoTracking()
            .Include(caseFile => caseFile.Case)
            .SingleOrDefaultAsync(caseFile => caseFile.Id == fileId && !caseFile.IsDeleted && !caseFile.Case.IsDeleted, cancellationToken);

        if (file is null)
        {
            return ApiResponse<SignedFileUrlDto>.Fail("File was not found.");
        }

        if (!await CanUseFilePermissionAsync(userId, PermissionNames.FilesDownload, file.CaseId, cancellationToken))
        {
            return ApiResponse<SignedFileUrlDto>.Fail("You do not have permission to download this file.");
        }

        var signedUrl = await _fileStorageService.GenerateSignedDownloadUrlAsync(file.Id, file.ObjectKey, file.OriginalFileName, cancellationToken);

        await _auditService.LogAsync(
            "CaseFile.DownloadUrlGenerated",
            "CaseFile",
            file.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            userId,
            newValues: JsonSerializer.Serialize(new
            {
                file.CaseId,
                file.Case.CaseNumber,
                file.OriginalFileName,
                file.FileType,
                signedUrl.ExpiresOnUtc
            }),
            cancellationToken: cancellationToken);

        return ApiResponse<SignedFileUrlDto>.Ok(signedUrl);
    }

    public async Task<ApiResponse<bool>> SoftDeleteCaseFileAsync(int fileId, string userId, CancellationToken cancellationToken = default)
    {
        var file = await _dbContext.CaseFiles
            .Include(caseFile => caseFile.Case)
            .SingleOrDefaultAsync(caseFile => caseFile.Id == fileId && !caseFile.IsDeleted && !caseFile.Case.IsDeleted, cancellationToken);

        if (file is null)
        {
            return ApiResponse<bool>.Fail("File was not found.");
        }

        if (!await CanUseFilePermissionAsync(userId, PermissionNames.FilesDeleteSoft, file.CaseId, cancellationToken))
        {
            return ApiResponse<bool>.Fail("You do not have permission to delete this file.");
        }

        file.IsDeleted = true;
        file.DeletedOn = DateTime.UtcNow;
        file.DeletedByUserId = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "CaseFile.SoftDeleted",
            "CaseFile",
            file.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            userId,
            oldValues: JsonSerializer.Serialize(new { file.CaseId, file.Case.CaseNumber, file.OriginalFileName, IsDeleted = false }),
            newValues: JsonSerializer.Serialize(new { file.CaseId, file.Case.CaseNumber, file.OriginalFileName, IsDeleted = true }),
            cancellationToken: cancellationToken);

        return ApiResponse<bool>.Ok(true, "File deleted successfully.");
    }

    private async Task<bool> CanUseFilePermissionAsync(string userId, string permissionName, int caseId, CancellationToken cancellationToken)
    {
        return await _permissionService.HasPermissionAsync(userId, permissionName, cancellationToken) &&
            await _caseAuthorizationService.CanAccessCaseAsync(userId, caseId, cancellationToken);
    }

    private List<string> ValidateFile(UploadCaseFileDto dto)
    {
        var errors = new List<string>();
        var extension = Path.GetExtension(dto.OriginalFileName);
        var maxBytes = Math.Max(1, _storageOptions.MaxFileSizeMb) * 1024L * 1024L;

        if (dto.Content is null)
        {
            errors.Add("File is required.");
        }

        if (dto.FileSizeBytes <= 0)
        {
            errors.Add("File is empty.");
        }

        if (dto.FileSizeBytes > maxBytes)
        {
            errors.Add($"File cannot exceed {_storageOptions.MaxFileSizeMb} MB.");
        }

        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            errors.Add("File extension is not allowed.");
        }

        if (DangerousExtensions.Contains(extension))
        {
            errors.Add("Dangerous file extension rejected.");
        }

        var normalizedContentType = NormalizeContentType(dto.ContentType, extension);
        if (!IsContentTypeAllowed(extension, normalizedContentType))
        {
            errors.Add("File content type is not allowed.");
        }

        return errors;
    }

    private static bool IsContentTypeAllowed(string extension, string contentType)
    {
        return extension.ToLowerInvariant() switch
        {
            ".stl" => contentType is "application/octet-stream" or "model/stl" or "application/sla",
            ".jpg" or ".jpeg" => contentType is "image/jpeg" or "application/octet-stream",
            ".png" => contentType is "image/png" or "application/octet-stream",
            ".pdf" => contentType is "application/pdf" or "application/octet-stream",
            ".zip" => contentType is "application/zip" or "application/x-zip-compressed" or "application/octet-stream",
            _ => false
        };
    }

    private static string NormalizeContentType(string? contentType, string extension)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            return contentType.Trim().ToLowerInvariant();
        }

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    private static string CreateSafeFileName(string originalNameWithoutExtension, string extension)
    {
        var safeName = new string(originalNameWithoutExtension
            .Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '_')
            .ToArray())
            .Trim('_');

        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "file";
        }

        return $"{safeName[..Math.Min(safeName.Length, 80)]}{extension.ToLowerInvariant()}";
    }

    private static CaseFileDto ToDto(CaseFile file)
    {
        return new CaseFileDto
        {
            Id = file.Id,
            CaseId = file.CaseId,
            FileType = file.FileType,
            OriginalFileName = file.OriginalFileName,
            FileSizeBytes = file.FileSizeBytes,
            ContentType = file.ContentType,
            FileExtension = file.FileExtension,
            Checksum = file.Checksum,
            UploadedByUserId = file.UploadedByUserId,
            UploadedOn = file.UploadedOn,
            IsDeleted = file.IsDeleted
        };
    }
}
