using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Sync;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Entities;
using SimpleCasePortal.Domain.Enums;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.ExternalApis;

public sealed class ExternalSyncService : IExternalSyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICaseAuthorizationService _caseAuthorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPermissionService _permissionService;
    private readonly IReadOnlyDictionary<SyncTargetEnum, IExternalApiClient> _clients;
    private readonly ExternalApisOptions _options;

    public ExternalSyncService(
        AppDbContext dbContext,
        IAuditService auditService,
        ICaseAuthorizationService caseAuthorizationService,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IPermissionService permissionService,
        IEnumerable<IExternalApiClient> clients,
        IOptions<ExternalApisOptions> options)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _caseAuthorizationService = caseAuthorizationService;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _permissionService = permissionService;
        _clients = clients.ToDictionary(client => client.SyncTarget);
        _options = options.Value;
    }

    public Task<ApiResponse<CaseSyncLogDto>> SyncCaseToDentistAsync(int caseId, CancellationToken cancellationToken = default)
    {
        return SyncCaseAsync(caseId, SyncTargetEnum.DentistApp, PermissionNames.SyncDentist, 0, cancellationToken);
    }

    public Task<ApiResponse<CaseSyncLogDto>> SyncCaseToWorkflowAsync(int caseId, CancellationToken cancellationToken = default)
    {
        return SyncCaseAsync(caseId, SyncTargetEnum.WorkflowApp, PermissionNames.SyncWorkflow, 0, cancellationToken);
    }

    public Task<ApiResponse<CaseSyncLogDto>> SyncCaseToProductionAsync(int caseId, CancellationToken cancellationToken = default)
    {
        return SyncCaseAsync(caseId, SyncTargetEnum.ProductionApp, PermissionNames.SyncProduction, 0, cancellationToken);
    }

    public async Task<ApiResponse<CaseSyncLogDto>> RetrySyncAsync(int syncLogId, CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        if (!await _permissionService.HasPermissionAsync(userId, PermissionNames.SyncRetry, cancellationToken))
        {
            return ApiResponse<CaseSyncLogDto>.Fail("You do not have permission to retry sync.");
        }

        var failedLog = await _dbContext.CaseSyncLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Id == syncLogId && log.SyncStatus == SyncStatusEnum.Failed, cancellationToken);

        if (failedLog is null)
        {
            return ApiResponse<CaseSyncLogDto>.Fail("Failed sync log was not found.");
        }

        var permissionName = GetPermissionName(failedLog.SyncTarget);
        var response = await SyncCaseAsync(
            failedLog.CaseId,
            failedLog.SyncTarget,
            permissionName,
            failedLog.RetryCount + 1,
            cancellationToken);

        if (response.Success && response.Data is not null)
        {
            await _auditService.LogAsync(
                "CaseSync.Retried",
                "CaseSyncLog",
                response.Data.Id.ToString(CultureInfo.InvariantCulture),
                userId,
                newValues: JsonSerializer.Serialize(new
                {
                    OriginalSyncLogId = syncLogId,
                    NewSyncLogId = response.Data.Id,
                    failedLog.SyncTarget,
                    response.Data.SyncStatus
                }, JsonOptions),
                cancellationToken: cancellationToken);
        }

        return response;
    }

    public async Task<ApiResponse<IReadOnlyCollection<CaseSyncStatusDto>>> GetCaseSyncStatusAsync(
        int caseId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        if (!await _caseAuthorizationService.CanAccessCaseAsync(userId, caseId, cancellationToken))
        {
            return ApiResponse<IReadOnlyCollection<CaseSyncStatusDto>>.Fail("You do not have permission to view sync status for this case.");
        }

        var logs = await _dbContext.CaseSyncLogs
            .AsNoTracking()
            .Where(log => log.CaseId == caseId)
            .OrderByDescending(log => log.SyncedOn)
            .ThenByDescending(log => log.Id)
            .ToArrayAsync(cancellationToken);

        var canRetry = await _permissionService.HasPermissionAsync(userId, PermissionNames.SyncRetry, cancellationToken);
        var statuses = Enum.GetValues<SyncTargetEnum>()
            .Select(target =>
            {
                var latest = logs.FirstOrDefault(log => log.SyncTarget == target);
                return new CaseSyncStatusDto
                {
                    SyncTarget = target,
                    SyncStatus = latest?.SyncStatus,
                    LastSyncedOn = latest?.SyncedOn,
                    ExternalReferenceId = latest?.ExternalReferenceId,
                    LastErrorMessage = latest?.ErrorMessage,
                    LatestSyncLogId = latest?.Id,
                    CanRetry = canRetry && latest?.SyncStatus == SyncStatusEnum.Failed
                };
            })
            .ToArray();

        return ApiResponse<IReadOnlyCollection<CaseSyncStatusDto>>.Ok(statuses);
    }

    public async Task<ApiResponse<IReadOnlyCollection<CaseSyncLogDto>>> GetCaseSyncLogsAsync(
        int caseId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        if (!await _caseAuthorizationService.CanAccessCaseAsync(userId, caseId, cancellationToken))
        {
            return ApiResponse<IReadOnlyCollection<CaseSyncLogDto>>.Fail("You do not have permission to view sync logs for this case.");
        }

        var logs = await _dbContext.CaseSyncLogs
            .AsNoTracking()
            .Where(log => log.CaseId == caseId)
            .OrderByDescending(log => log.SyncedOn)
            .Select(log => ToDto(log))
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CaseSyncLogDto>>.Ok(logs);
    }

    private async Task<ApiResponse<CaseSyncLogDto>> SyncCaseAsync(
        int caseId,
        SyncTargetEnum syncTarget,
        string permissionName,
        int retryCount,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        if (!await _permissionService.HasPermissionAsync(userId, permissionName, cancellationToken))
        {
            return ApiResponse<CaseSyncLogDto>.Fail($"You do not have permission to sync to {GetTargetDisplayName(syncTarget)}.");
        }

        if (!await _caseAuthorizationService.CanAccessCaseAsync(userId, caseId, cancellationToken))
        {
            return ApiResponse<CaseSyncLogDto>.Fail("You do not have permission to sync this case.");
        }

        var caseEntity = await _dbContext.Cases
            .Include(entity => entity.DoctorClinic)
            .Include(entity => entity.Files)
            .SingleOrDefaultAsync(entity => entity.Id == caseId && !entity.IsDeleted, cancellationToken);

        if (caseEntity is null)
        {
            return ApiResponse<CaseSyncLogDto>.Fail("Case was not found.");
        }

        var activeFiles = caseEntity.Files
            .Where(file => !file.IsDeleted)
            .OrderBy(file => file.FileType)
            .ThenBy(file => file.UploadedOn)
            .ToArray();

        if (!activeFiles.Any(file => string.Equals(file.FileExtension, ".stl", StringComparison.OrdinalIgnoreCase)))
        {
            return ApiResponse<CaseSyncLogDto>.Fail("At least one active STL file is required before syncing this case.");
        }

        if (!_clients.TryGetValue(syncTarget, out var client))
        {
            return ApiResponse<CaseSyncLogDto>.Fail($"{GetTargetDisplayName(syncTarget)} sync client is not configured.");
        }

        var safePayload = BuildPayload(caseEntity, activeFiles, includeSignedUrls: false);
        var outboundPayload = await BuildOutboundPayloadAsync(caseEntity, activeFiles, syncTarget, cancellationToken);
        var now = DateTime.UtcNow;
        var syncLog = new CaseSyncLog
        {
            CaseId = caseId,
            SyncTarget = syncTarget,
            SyncStatus = SyncStatusEnum.Pending,
            RequestPayload = JsonSerializer.Serialize(safePayload, JsonOptions),
            SyncedByUserId = userId,
            SyncedOn = now,
            RetryCount = retryCount
        };

        await _dbContext.CaseSyncLogs.AddAsync(syncLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "CaseSync.Started",
            "Case",
            caseId.ToString(CultureInfo.InvariantCulture),
            userId,
            newValues: JsonSerializer.Serialize(new
            {
                caseEntity.CaseNumber,
                SyncTarget = syncTarget,
                RetryCount = retryCount
            }, JsonOptions),
            cancellationToken: cancellationToken);

        var result = await client.SendCaseAsync(outboundPayload, cancellationToken);

        syncLog.SyncStatus = result.Success ? SyncStatusEnum.Success : SyncStatusEnum.Failed;
        syncLog.ResponsePayload = SanitizePayload(result.ResponsePayload);
        syncLog.ErrorMessage = result.Success ? null : Truncate(result.Message, 4000);
        syncLog.ExternalReferenceId = result.Success ? Truncate(result.ExternalReferenceId, 250) : null;
        syncLog.SyncedOn = DateTime.UtcNow;

        caseEntity.Status = result.Success
            ? GetSuccessCaseStatus(syncTarget)
            : CaseStatusEnum.SyncFailed;
        caseEntity.UpdatedOn = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            result.Success ? "CaseSync.Success" : "CaseSync.Failed",
            "CaseSyncLog",
            syncLog.Id.ToString(CultureInfo.InvariantCulture),
            userId,
            newValues: JsonSerializer.Serialize(new
            {
                caseEntity.CaseNumber,
                SyncTarget = syncTarget,
                SyncStatus = syncLog.SyncStatus,
                syncLog.ExternalReferenceId,
                ErrorMessage = syncLog.ErrorMessage
            }, JsonOptions),
            cancellationToken: cancellationToken);

        var message = result.Success
            ? $"{GetTargetDisplayName(syncTarget)} sync completed successfully."
            : $"{GetTargetDisplayName(syncTarget)} sync failed: {result.Message}";

        return result.Success
            ? ApiResponse<CaseSyncLogDto>.Ok(ToDto(syncLog), message)
            : ApiResponse<CaseSyncLogDto>.Fail(message);
    }

    private async Task<ExternalCaseSyncPayloadDto> BuildOutboundPayloadAsync(
        Case caseEntity,
        IReadOnlyCollection<CaseFile> activeFiles,
        SyncTargetEnum syncTarget,
        CancellationToken cancellationToken)
    {
        var targetOptions = GetTargetOptions(syncTarget);
        if (!targetOptions.SendSignedFileUrls)
        {
            return BuildPayload(caseEntity, activeFiles, includeSignedUrls: false);
        }

        var payload = BuildPayload(caseEntity, activeFiles, includeSignedUrls: false);
        foreach (var file in payload.Files)
        {
            var sourceFile = activeFiles.Single(activeFile => activeFile.ObjectKey == file.ObjectKey);
            var signedUrl = await _fileStorageService.GenerateSignedDownloadUrlAsync(
                sourceFile.Id,
                sourceFile.ObjectKey,
                sourceFile.OriginalFileName,
                cancellationToken);
            file.DownloadUrl = signedUrl.Url;
        }

        return payload;
    }

    private static ExternalCaseSyncPayloadDto BuildPayload(
        Case caseEntity,
        IReadOnlyCollection<CaseFile> activeFiles,
        bool includeSignedUrls)
    {
        return new ExternalCaseSyncPayloadDto
        {
            CaseNumber = caseEntity.CaseNumber,
            Patient = new ExternalPatientDto
            {
                Name = caseEntity.PatientName,
                Age = caseEntity.Age,
                Gender = caseEntity.Gender,
                DateOfBirth = caseEntity.DateOfBirth
            },
            DoctorClinic = new ExternalDoctorClinicDto
            {
                Id = caseEntity.DoctorClinicId,
                Name = caseEntity.DoctorClinic.Name,
                Email = caseEntity.DoctorClinic.Email
            },
            Files = activeFiles.Select(file => new ExternalCaseFileDto
            {
                FileType = file.FileType,
                OriginalFileName = file.OriginalFileName,
                ObjectKey = file.ObjectKey,
                FileSizeBytes = file.FileSizeBytes,
                Checksum = file.Checksum,
                DownloadUrl = includeSignedUrls ? string.Empty : null
            }).ToArray(),
            Notes = caseEntity.Notes,
            CreatedOn = caseEntity.CreatedOn
        };
    }

    private ExternalApiTargetOptions GetTargetOptions(SyncTargetEnum syncTarget)
    {
        return syncTarget switch
        {
            SyncTargetEnum.DentistApp => _options.DentistApp,
            SyncTargetEnum.WorkflowApp => _options.WorkflowApp,
            SyncTargetEnum.ProductionApp => _options.ProductionApp,
            _ => new ExternalApiTargetOptions()
        };
    }

    private static CaseStatusEnum GetSuccessCaseStatus(SyncTargetEnum syncTarget)
    {
        return syncTarget switch
        {
            SyncTargetEnum.DentistApp => CaseStatusEnum.SyncedToDentist,
            SyncTargetEnum.WorkflowApp => CaseStatusEnum.SyncedToWorkflow,
            SyncTargetEnum.ProductionApp => CaseStatusEnum.SyncedToProduction,
            _ => CaseStatusEnum.Submitted
        };
    }

    private static string GetPermissionName(SyncTargetEnum syncTarget)
    {
        return syncTarget switch
        {
            SyncTargetEnum.DentistApp => PermissionNames.SyncDentist,
            SyncTargetEnum.WorkflowApp => PermissionNames.SyncWorkflow,
            SyncTargetEnum.ProductionApp => PermissionNames.SyncProduction,
            _ => throw new InvalidOperationException("Unsupported sync target.")
        };
    }

    private static string GetTargetDisplayName(SyncTargetEnum syncTarget)
    {
        return syncTarget switch
        {
            SyncTargetEnum.DentistApp => "Dentist App",
            SyncTargetEnum.WorkflowApp => "Workflow App",
            SyncTargetEnum.ProductionApp => "Production App",
            _ => syncTarget.ToString()
        };
    }

    private string RequireUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new InvalidOperationException("Authenticated user id is missing.");
        }

        return _currentUserService.UserId;
    }

    private static string? SanitizePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(payload);
            MaskDownloadUrls(node);
            return node?.ToJsonString(JsonOptions);
        }
        catch (JsonException)
        {
            return payload.Contains("downloadUrl", StringComparison.OrdinalIgnoreCase)
                ? "[Response payload omitted because it may contain signed URLs.]"
                : payload;
        }
    }

    private static void MaskDownloadUrls(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var propertyName in jsonObject.Select(pair => pair.Key).ToArray())
            {
                if (string.Equals(propertyName, "downloadUrl", StringComparison.OrdinalIgnoreCase))
                {
                    jsonObject[propertyName] = null;
                }
                else
                {
                    MaskDownloadUrls(jsonObject[propertyName]);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var child in jsonArray)
            {
                MaskDownloadUrls(child);
            }
        }
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static CaseSyncLogDto ToDto(CaseSyncLog log)
    {
        return new CaseSyncLogDto
        {
            Id = log.Id,
            CaseId = log.CaseId,
            SyncTarget = log.SyncTarget,
            SyncStatus = log.SyncStatus,
            ErrorMessage = log.ErrorMessage,
            ExternalReferenceId = log.ExternalReferenceId,
            SyncedByUserId = log.SyncedByUserId,
            SyncedOn = log.SyncedOn,
            RetryCount = log.RetryCount
        };
    }
}
