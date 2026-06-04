using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Dashboard;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Enums;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public DashboardService(AppDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<ApiResponse<DashboardSummaryDto>> GetDashboardAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var canViewAllCases = await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewAll, cancellationToken);
        var canViewOwnCases = await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewOwn, cancellationToken);
        var canCreateCase = await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesCreate, cancellationToken);
        var canViewAudit = await _permissionService.HasPermissionAsync(userId, PermissionNames.AuditView, cancellationToken);

        var summary = new DashboardSummaryDto
        {
            CanViewCases = canViewAllCases || canViewOwnCases,
            CanCreateCase = canCreateCase,
            CanViewAudit = canViewAudit
        };

        if (!summary.CanViewCases)
        {
            if (canViewAudit)
            {
                summary.RecentAuditActivities = await GetRecentAuditActivitiesAsync(cancellationToken);
            }

            return ApiResponse<DashboardSummaryDto>.Ok(summary);
        }

        var casesQuery = _dbContext.Cases
            .AsNoTracking()
            .Include(caseEntity => caseEntity.DoctorClinic)
            .Where(caseEntity => !caseEntity.IsDeleted);

        if (!canViewAllCases)
        {
            var doctorClinicId = await _dbContext.ApplicationUsers
                .AsNoTracking()
                .Where(user => user.Id == userId && user.IsActive && !user.IsDeleted)
                .Select(user => user.DoctorClinicId)
                .SingleOrDefaultAsync(cancellationToken);

            if (doctorClinicId is null)
            {
                return ApiResponse<DashboardSummaryDto>.Ok(summary);
            }

            casesQuery = casesQuery.Where(caseEntity => caseEntity.DoctorClinicId == doctorClinicId.Value);
        }

        var visibleCaseIds = casesQuery.Select(caseEntity => caseEntity.Id);

        summary.TotalCases = await casesQuery.CountAsync(cancellationToken);
        summary.DraftCases = await casesQuery.CountAsync(caseEntity => caseEntity.Status == CaseStatusEnum.Draft, cancellationToken);
        summary.SubmittedCases = await casesQuery.CountAsync(caseEntity => caseEntity.Status == CaseStatusEnum.Submitted, cancellationToken);
        summary.SyncedCases = await casesQuery.CountAsync(caseEntity =>
            caseEntity.Status == CaseStatusEnum.SyncedToDentist ||
            caseEntity.Status == CaseStatusEnum.SyncedToWorkflow ||
            caseEntity.Status == CaseStatusEnum.SyncedToProduction,
            cancellationToken);
        summary.FailedSyncCases = await casesQuery.CountAsync(caseEntity => caseEntity.Status == CaseStatusEnum.SyncFailed, cancellationToken);
        summary.TotalUploadedFiles = await _dbContext.CaseFiles
            .AsNoTracking()
            .CountAsync(file => !file.IsDeleted && visibleCaseIds.Contains(file.CaseId), cancellationToken);

        summary.RecentCases = await casesQuery
            .OrderByDescending(caseEntity => caseEntity.CreatedOn)
            .ThenByDescending(caseEntity => caseEntity.Id)
            .Take(6)
            .Select(caseEntity => new DashboardCaseDto
            {
                Id = caseEntity.Id,
                CaseNumber = caseEntity.CaseNumber,
                PatientName = caseEntity.PatientName,
                DoctorClinicName = caseEntity.DoctorClinic.Name,
                Status = caseEntity.Status,
                CreatedOn = caseEntity.CreatedOn
            })
            .ToArrayAsync(cancellationToken);

        summary.RecentSyncFailures = await _dbContext.CaseSyncLogs
            .AsNoTracking()
            .Include(log => log.Case)
            .Where(log =>
                log.SyncStatus == SyncStatusEnum.Failed &&
                visibleCaseIds.Contains(log.CaseId))
            .OrderByDescending(log => log.SyncedOn)
            .ThenByDescending(log => log.Id)
            .Take(5)
            .Select(log => new DashboardSyncFailureDto
            {
                Id = log.Id,
                CaseId = log.CaseId,
                CaseNumber = log.Case.CaseNumber,
                SyncTarget = log.SyncTarget,
                ErrorMessage = log.ErrorMessage,
                SyncedOn = log.SyncedOn
            })
            .ToArrayAsync(cancellationToken);

        if (canViewAudit)
        {
            summary.RecentAuditActivities = await GetRecentAuditActivitiesAsync(cancellationToken);
        }

        return ApiResponse<DashboardSummaryDto>.Ok(summary);
    }

    private async Task<IReadOnlyCollection<DashboardAuditActivityDto>> GetRecentAuditActivitiesAsync(CancellationToken cancellationToken)
    {
        var logs = await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedOn)
            .ThenByDescending(log => log.Id)
            .Take(5)
            .ToArrayAsync(cancellationToken);

        var userIds = logs
            .Where(log => !string.IsNullOrWhiteSpace(log.UserId))
            .Select(log => log.UserId!)
            .Distinct()
            .ToArray();

        var users = userIds.Length == 0
            ? []
            : await _dbContext.ApplicationUsers
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => user.FullName, cancellationToken);

        return logs.Select(log => new DashboardAuditActivityDto
        {
            Id = log.Id,
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            UserDisplayName = string.IsNullOrWhiteSpace(log.UserId)
                ? "System"
                : users.GetValueOrDefault(log.UserId, log.UserId),
            CreatedOn = log.CreatedOn
        }).ToArray();
    }
}
