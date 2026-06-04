using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Audit;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Entities;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Security;

public sealed class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _dbContext;

    public AuditLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<AuditLogListDto>> GetAuditLogsAsync(
        AuditLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        NormalizeFilter(filter);

        var query = ApplyFilters(_dbContext.AuditLogs.AsNoTracking(), filter);
        var totalCount = await query.CountAsync(cancellationToken);
        var logs = await query
            .OrderByDescending(log => log.CreatedOn)
            .ThenByDescending(log => log.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        var userNames = await GetUserDisplayNamesAsync(logs.Select(log => log.UserId), cancellationToken);

        return ApiResponse<AuditLogListDto>.Ok(new AuditLogListDto
        {
            Logs = logs.Select(log => ToDto(log, userNames)).ToArray(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<ApiResponse<AuditLogDetailsDto>> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var log = await _dbContext.AuditLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(auditLog => auditLog.Id == id, cancellationToken);

        if (log is null)
        {
            return ApiResponse<AuditLogDetailsDto>.Fail("Audit log was not found.");
        }

        var userNames = await GetUserDisplayNamesAsync([log.UserId], cancellationToken);
        return ApiResponse<AuditLogDetailsDto>.Ok(ToDetailsDto(log, userNames));
    }

    public async Task<ApiResponse<IReadOnlyCollection<AuditOptionDto>>> GetAuditActionOptionsAsync(CancellationToken cancellationToken = default)
    {
        var actions = await _dbContext.AuditLogs
            .AsNoTracking()
            .Select(log => log.Action)
            .Distinct()
            .OrderBy(action => action)
            .Select(action => new AuditOptionDto { Value = action, Text = action })
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<AuditOptionDto>>.Ok(actions);
    }

    public async Task<ApiResponse<IReadOnlyCollection<AuditOptionDto>>> GetEntityNameOptionsAsync(CancellationToken cancellationToken = default)
    {
        var entityNames = await _dbContext.AuditLogs
            .AsNoTracking()
            .Select(log => log.EntityName)
            .Distinct()
            .OrderBy(entityName => entityName)
            .Select(entityName => new AuditOptionDto { Value = entityName, Text = entityName })
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<AuditOptionDto>>.Ok(entityNames);
    }

    public async Task<ApiResponse<IReadOnlyCollection<AuditOptionDto>>> GetUserOptionsAsync(CancellationToken cancellationToken = default)
    {
        var userIds = await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.UserId != null && log.UserId != string.Empty)
            .Select(log => log.UserId!)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var userNames = await GetUserDisplayNamesAsync(userIds, cancellationToken);
        var users = userIds
            .Select(userId => new AuditOptionDto { Value = userId, Text = userNames.GetValueOrDefault(userId, userId) })
            .OrderBy(user => user.Text)
            .ToArray();

        return ApiResponse<IReadOnlyCollection<AuditOptionDto>>.Ok(users);
    }

    private static IQueryable<AuditLog> ApplyFilters(IQueryable<AuditLog> query, AuditLogFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(log => log.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            query = query.Where(log => log.EntityName == filter.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserId))
        {
            query = query.Where(log => log.UserId == filter.UserId);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityId))
        {
            query = query.Where(log => log.EntityId.Contains(filter.EntityId));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            query = query.Where(log =>
                log.Action.Contains(filter.SearchText) ||
                log.EntityName.Contains(filter.SearchText) ||
                log.EntityId.Contains(filter.SearchText));
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(log => log.CreatedOn >= filter.DateFrom.Value.Date);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(log => log.CreatedOn < filter.DateTo.Value.Date.AddDays(1));
        }

        return query;
    }

    private async Task<Dictionary<string, string>> GetUserDisplayNamesAsync(
        IEnumerable<string?> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Select(userId => userId!)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        return await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => $"{user.FullName} ({user.Email})",
                cancellationToken);
    }

    private static void NormalizeFilter(AuditLogFilterDto filter)
    {
        filter.Action = NormalizeOptional(filter.Action);
        filter.EntityName = NormalizeOptional(filter.EntityName);
        filter.UserId = NormalizeOptional(filter.UserId);
        filter.EntityId = NormalizeOptional(filter.EntityId);
        filter.SearchText = NormalizeOptional(filter.SearchText);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AuditLogDto ToDto(AuditLog log, IReadOnlyDictionary<string, string> userNames)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserDisplayName = GetDisplayName(log.UserId, userNames),
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            IpAddress = log.IpAddress,
            Summary = SensitiveAuditValueMasker.CreateSummary(log.OldValues, log.NewValues),
            CreatedOn = log.CreatedOn
        };
    }

    private static AuditLogDetailsDto ToDetailsDto(AuditLog log, IReadOnlyDictionary<string, string> userNames)
    {
        return new AuditLogDetailsDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserDisplayName = GetDisplayName(log.UserId, userNames),
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            IpAddress = log.IpAddress,
            UserAgent = SensitiveAuditValueMasker.MaskAndFormat(log.UserAgent),
            OldValues = SensitiveAuditValueMasker.MaskAndFormat(log.OldValues),
            NewValues = SensitiveAuditValueMasker.MaskAndFormat(log.NewValues),
            CreatedOn = log.CreatedOn
        };
    }

    private static string GetDisplayName(string? userId, IReadOnlyDictionary<string, string> userNames)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return "System";
        }

        return userNames.GetValueOrDefault(userId, userId);
    }
}
