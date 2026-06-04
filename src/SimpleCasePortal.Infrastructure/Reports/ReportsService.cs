using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Reports;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Entities;
using SimpleCasePortal.Domain.Enums;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Reports;

public sealed class ReportsService : IReportsService
{
    private readonly AppDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public ReportsService(AppDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<ApiResponse<CaseSummaryReportDto>> GetCaseSummaryReportAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(userId, filter, cancellationToken);
        if (!scope.CanViewReports)
        {
            return ApiResponse<CaseSummaryReportDto>.Fail("You do not have permission to view reports.");
        }

        var casesQuery = BuildCasesQuery(scope);

        var summary = new CaseSummaryReportDto
        {
            TotalCases = await casesQuery.CountAsync(cancellationToken),
            DraftCases = await casesQuery.CountAsync(caseEntity => caseEntity.Status == CaseStatusEnum.Draft, cancellationToken),
            SubmittedCases = await casesQuery.CountAsync(caseEntity => caseEntity.Status == CaseStatusEnum.Submitted, cancellationToken),
            SyncedCases = await casesQuery.CountAsync(caseEntity =>
                caseEntity.Status == CaseStatusEnum.SyncedToDentist ||
                caseEntity.Status == CaseStatusEnum.SyncedToWorkflow ||
                caseEntity.Status == CaseStatusEnum.SyncedToProduction,
                cancellationToken),
            FailedSyncCases = await casesQuery.CountAsync(caseEntity => caseEntity.Status == CaseStatusEnum.SyncFailed, cancellationToken),
            ArchivedCases = await casesQuery.CountAsync(caseEntity => caseEntity.Status == CaseStatusEnum.Archived, cancellationToken)
        };

        return ApiResponse<CaseSummaryReportDto>.Ok(summary);
    }

    public async Task<ApiResponse<IReadOnlyCollection<CaseStatusReportItemDto>>> GetCaseStatusReportAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(userId, filter, cancellationToken);
        if (!scope.CanViewReports)
        {
            return ApiResponse<IReadOnlyCollection<CaseStatusReportItemDto>>.Fail("You do not have permission to view reports.");
        }

        var grouped = await BuildCasesQuery(scope)
            .GroupBy(caseEntity => caseEntity.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .OrderBy(item => item.Status)
            .ToArrayAsync(cancellationToken);

        var total = grouped.Sum(item => item.Count);
        var report = grouped.Select(item => new CaseStatusReportItemDto
        {
            Status = item.Status,
            Count = item.Count,
            Percentage = total == 0 ? 0 : Math.Round(item.Count * 100m / total, 2)
        }).ToArray();

        return ApiResponse<IReadOnlyCollection<CaseStatusReportItemDto>>.Ok(report);
    }

    public async Task<ApiResponse<IReadOnlyCollection<DoctorClinicActivityReportItemDto>>> GetDoctorClinicActivityReportAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(userId, filter, cancellationToken);
        if (!scope.CanViewReports)
        {
            return ApiResponse<IReadOnlyCollection<DoctorClinicActivityReportItemDto>>.Fail("You do not have permission to view reports.");
        }

        var casesQuery = BuildCasesQuery(scope);
        var visibleCaseIds = BuildCasesQuery(scope, applyDateFilter: false).Select(caseEntity => caseEntity.Id);

        var caseActivity = await casesQuery
            .GroupBy(caseEntity => new { caseEntity.DoctorClinicId, caseEntity.DoctorClinic.Name })
            .Select(group => new DoctorClinicActivityReportItemDto
            {
                DoctorClinicId = group.Key.DoctorClinicId,
                DoctorClinicName = group.Key.Name,
                TotalCases = group.Count(),
                LastCaseDate = group.Max(caseEntity => caseEntity.CreatedOn)
            })
            .ToDictionaryAsync(item => item.DoctorClinicId, cancellationToken);

        var fileActivity = await BuildFilesQuery(scope, visibleCaseIds)
            .GroupBy(file => new { file.Case.DoctorClinicId, file.Case.DoctorClinic.Name })
            .Select(group => new
            {
                group.Key.DoctorClinicId,
                DoctorClinicName = group.Key.Name,
                UploadedFiles = group.Count()
            })
            .ToArrayAsync(cancellationToken);

        var syncActivity = await BuildSyncLogsQuery(scope, visibleCaseIds)
            .GroupBy(log => new { log.Case.DoctorClinicId, log.Case.DoctorClinic.Name })
            .Select(group => new
            {
                group.Key.DoctorClinicId,
                DoctorClinicName = group.Key.Name,
                SuccessfulSyncCount = group.Count(log => log.SyncStatus == SyncStatusEnum.Success),
                FailedSyncCount = group.Count(log => log.SyncStatus == SyncStatusEnum.Failed)
            })
            .ToArrayAsync(cancellationToken);

        foreach (var file in fileActivity)
        {
            if (!caseActivity.TryGetValue(file.DoctorClinicId, out var item))
            {
                item = new DoctorClinicActivityReportItemDto
                {
                    DoctorClinicId = file.DoctorClinicId,
                    DoctorClinicName = file.DoctorClinicName
                };
                caseActivity.Add(item.DoctorClinicId, item);
            }

            item.UploadedFiles = file.UploadedFiles;
        }

        foreach (var sync in syncActivity)
        {
            if (!caseActivity.TryGetValue(sync.DoctorClinicId, out var item))
            {
                item = new DoctorClinicActivityReportItemDto
                {
                    DoctorClinicId = sync.DoctorClinicId,
                    DoctorClinicName = sync.DoctorClinicName
                };
                caseActivity.Add(item.DoctorClinicId, item);
            }

            item.SuccessfulSyncCount = sync.SuccessfulSyncCount;
            item.FailedSyncCount = sync.FailedSyncCount;
        }

        var report = caseActivity.Values
            .OrderByDescending(item => item.TotalCases)
            .ThenBy(item => item.DoctorClinicName)
            .ToArray();

        return ApiResponse<IReadOnlyCollection<DoctorClinicActivityReportItemDto>>.Ok(report);
    }

    public async Task<ApiResponse<FileUploadReportDto>> GetFileUploadReportAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(userId, filter, cancellationToken);
        if (!scope.CanViewReports)
        {
            return ApiResponse<FileUploadReportDto>.Fail("You do not have permission to view reports.");
        }

        var casesQuery = BuildCasesQuery(scope, applyDateFilter: false);
        var visibleCaseIds = casesQuery.Select(caseEntity => caseEntity.Id);
        var filesQuery = BuildFilesQuery(scope, visibleCaseIds);

        var report = new FileUploadReportDto
        {
            TotalFiles = await filesQuery.CountAsync(cancellationToken),
            TotalFileSizeBytes = await filesQuery.SumAsync(file => (long?)file.FileSizeBytes, cancellationToken) ?? 0,
            StlFileCount = await filesQuery.CountAsync(file =>
                file.FileType == FileTypeEnum.UpperSTL ||
                file.FileType == FileTypeEnum.LowerSTL ||
                file.FileType == FileTypeEnum.BiteSTL,
                cancellationToken),
            FilesByType = await filesQuery
                .GroupBy(file => file.FileType)
                .Select(group => new FileTypeReportItemDto
                {
                    FileType = group.Key,
                    Count = group.Count(),
                    TotalFileSizeBytes = group.Sum(file => file.FileSizeBytes)
                })
                .OrderBy(item => item.FileType)
                .ToArrayAsync(cancellationToken),
            UploadsByDate = await filesQuery
                .GroupBy(file => file.UploadedOn.Date)
                .Select(group => new FileUploadDateReportItemDto
                {
                    Date = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(item => item.Date)
                .Take(14)
                .ToArrayAsync(cancellationToken)
        };

        return ApiResponse<FileUploadReportDto>.Ok(report);
    }

    public async Task<ApiResponse<IReadOnlyCollection<SyncReportItemDto>>> GetSyncReportAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(userId, filter, cancellationToken);
        if (!scope.CanViewReports)
        {
            return ApiResponse<IReadOnlyCollection<SyncReportItemDto>>.Fail("You do not have permission to view reports.");
        }

        var casesQuery = BuildCasesQuery(scope, applyDateFilter: false);
        var visibleCaseIds = casesQuery.Select(caseEntity => caseEntity.Id);
        var report = await BuildSyncLogsQuery(scope, visibleCaseIds)
            .GroupBy(log => log.SyncTarget)
            .Select(group => new SyncReportItemDto
            {
                SyncTarget = group.Key,
                SuccessCount = group.Count(log => log.SyncStatus == SyncStatusEnum.Success),
                FailedCount = group.Count(log => log.SyncStatus == SyncStatusEnum.Failed),
                LastSuccessDate = group
                    .Where(log => log.SyncStatus == SyncStatusEnum.Success)
                    .Max(log => (DateTime?)log.SyncedOn),
                LastFailureDate = group
                    .Where(log => log.SyncStatus == SyncStatusEnum.Failed)
                    .Max(log => (DateTime?)log.SyncedOn)
            })
            .OrderBy(item => item.SyncTarget)
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<SyncReportItemDto>>.Ok(report);
    }

    public async Task<ApiResponse<IReadOnlyCollection<FailedSyncReportItemDto>>> GetFailedSyncReportAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var scope = await BuildScopeAsync(userId, filter, cancellationToken);
        if (!scope.CanViewReports)
        {
            return ApiResponse<IReadOnlyCollection<FailedSyncReportItemDto>>.Fail("You do not have permission to view reports.");
        }

        var casesQuery = BuildCasesQuery(scope, applyDateFilter: false);
        var visibleCaseIds = casesQuery.Select(caseEntity => caseEntity.Id);
        var report = await BuildSyncLogsQuery(scope, visibleCaseIds)
            .Where(log => log.SyncStatus == SyncStatusEnum.Failed)
            .OrderByDescending(log => log.SyncedOn)
            .ThenByDescending(log => log.Id)
            .Take(50)
            .Select(log => new FailedSyncReportItemDto
            {
                SyncLogId = log.Id,
                CaseId = log.CaseId,
                CaseNumber = log.Case.CaseNumber,
                PatientName = log.Case.PatientName,
                DoctorClinicName = log.Case.DoctorClinic.Name,
                SyncTarget = log.SyncTarget,
                ErrorMessage = log.ErrorMessage,
                LastFailedOn = log.SyncedOn
            })
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<FailedSyncReportItemDto>>.Ok(report);
    }

    public async Task<ApiResponse<IReadOnlyCollection<DoctorClinicReportOptionDto>>> GetDoctorClinicOptionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var canViewReports = await _permissionService.HasPermissionAsync(userId, PermissionNames.ReportsView, cancellationToken);
        if (!canViewReports)
        {
            return ApiResponse<IReadOnlyCollection<DoctorClinicReportOptionDto>>.Fail("You do not have permission to view reports.");
        }

        var canViewAll = await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewAll, cancellationToken);
        if (!canViewAll)
        {
            return ApiResponse<IReadOnlyCollection<DoctorClinicReportOptionDto>>.Ok([]);
        }

        var options = await _dbContext.DoctorClinics
            .AsNoTracking()
            .Where(doctorClinic => !doctorClinic.IsDeleted)
            .OrderBy(doctorClinic => doctorClinic.Name)
            .Select(doctorClinic => new DoctorClinicReportOptionDto
            {
                Id = doctorClinic.Id,
                Name = doctorClinic.Name
            })
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<DoctorClinicReportOptionDto>>.Ok(options);
    }

    public async Task<bool> CanFilterByDoctorClinicAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _permissionService.HasPermissionAsync(userId, PermissionNames.ReportsView, cancellationToken) &&
            await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewAll, cancellationToken);
    }

    private async Task<ReportScope> BuildScopeAsync(string userId, ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var canViewReports = await _permissionService.HasPermissionAsync(userId, PermissionNames.ReportsView, cancellationToken);
        var canViewAll = canViewReports &&
            await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewAll, cancellationToken);

        var userDoctorClinicId = canViewAll
            ? null
            : await _dbContext.ApplicationUsers
                .AsNoTracking()
                .Where(user => user.Id == userId && user.IsActive && !user.IsDeleted)
                .Select(user => user.DoctorClinicId)
                .SingleOrDefaultAsync(cancellationToken);

        var normalized = NormalizeFilter(filter);
        var effectiveDoctorClinicId = canViewAll
            ? normalized.DoctorClinicId
            : userDoctorClinicId;

        return new ReportScope(
            canViewReports,
            canViewAll,
            effectiveDoctorClinicId,
            normalized.DateFrom!.Value,
            normalized.DateTo!.Value.Date.AddDays(1),
            normalized.CaseStatus,
            normalized.SyncTarget,
            normalized.SyncStatus,
            normalized.SearchText);
    }

    private static ReportFilterDto NormalizeFilter(ReportFilterDto filter)
    {
        var today = DateTime.UtcNow.Date;
        var dateFrom = filter.DateFrom?.Date ?? today.AddDays(-30);
        var dateTo = filter.DateTo?.Date ?? today;

        if (dateTo < dateFrom)
        {
            dateTo = dateFrom;
        }

        return new ReportFilterDto
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            DoctorClinicId = filter.DoctorClinicId,
            CaseStatus = filter.CaseStatus,
            SyncTarget = filter.SyncTarget,
            SyncStatus = filter.SyncStatus,
            SearchText = string.IsNullOrWhiteSpace(filter.SearchText) ? null : filter.SearchText.Trim()
        };
    }

    private IQueryable<Case> BuildCasesQuery(ReportScope scope, bool applyDateFilter = true)
    {
        var query = _dbContext.Cases
            .AsNoTracking()
            .Include(caseEntity => caseEntity.DoctorClinic)
            .Where(caseEntity => !caseEntity.IsDeleted);

        if (applyDateFilter)
        {
            query = query.Where(caseEntity =>
                caseEntity.CreatedOn >= scope.DateFrom &&
                caseEntity.CreatedOn < scope.DateToExclusive);
        }

        if (scope.DoctorClinicId.HasValue)
        {
            query = query.Where(caseEntity => caseEntity.DoctorClinicId == scope.DoctorClinicId.Value);
        }
        else if (!scope.CanViewAll)
        {
            query = query.Where(_ => false);
        }

        if (scope.CaseStatus.HasValue)
        {
            query = query.Where(caseEntity => caseEntity.Status == scope.CaseStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(scope.SearchText))
        {
            query = query.Where(caseEntity =>
                caseEntity.CaseNumber.Contains(scope.SearchText) ||
                caseEntity.PatientName.Contains(scope.SearchText) ||
                caseEntity.DoctorClinic.Name.Contains(scope.SearchText));
        }

        return query;
    }

    private IQueryable<CaseFile> BuildFilesQuery(ReportScope scope, IQueryable<int> visibleCaseIds)
    {
        return _dbContext.CaseFiles
            .AsNoTracking()
            .Include(file => file.Case)
            .ThenInclude(caseEntity => caseEntity.DoctorClinic)
            .Where(file =>
                !file.IsDeleted &&
                !file.Case.IsDeleted &&
                visibleCaseIds.Contains(file.CaseId) &&
                file.UploadedOn >= scope.DateFrom &&
                file.UploadedOn < scope.DateToExclusive);
    }

    private IQueryable<CaseSyncLog> BuildSyncLogsQuery(ReportScope scope, IQueryable<int> visibleCaseIds)
    {
        var query = _dbContext.CaseSyncLogs
            .AsNoTracking()
            .Include(log => log.Case)
            .ThenInclude(caseEntity => caseEntity.DoctorClinic)
            .Where(log =>
                !log.Case.IsDeleted &&
                visibleCaseIds.Contains(log.CaseId) &&
                log.SyncedOn >= scope.DateFrom &&
                log.SyncedOn < scope.DateToExclusive);

        if (scope.SyncTarget.HasValue)
        {
            query = query.Where(log => log.SyncTarget == scope.SyncTarget.Value);
        }

        if (scope.SyncStatus.HasValue)
        {
            query = query.Where(log => log.SyncStatus == scope.SyncStatus.Value);
        }

        return query;
    }

    private sealed record ReportScope(
        bool CanViewReports,
        bool CanViewAll,
        int? DoctorClinicId,
        DateTime DateFrom,
        DateTime DateToExclusive,
        CaseStatusEnum? CaseStatus,
        SyncTargetEnum? SyncTarget,
        SyncStatusEnum? SyncStatus,
        string? SearchText);
}
