using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Security;

public sealed class CaseAuthorizationService : ICaseAuthorizationService
{
    private readonly AppDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public CaseAuthorizationService(AppDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<bool> CanAccessCaseAsync(string userId, int caseId, CancellationToken cancellationToken = default)
    {
        if (await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewAll, cancellationToken))
        {
            return true;
        }

        if (!await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewOwn, cancellationToken))
        {
            return false;
        }

        var user = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .SingleOrDefaultAsync(applicationUser => applicationUser.Id == userId && !applicationUser.IsDeleted && applicationUser.IsActive, cancellationToken);

        if (user?.DoctorClinicId is null)
        {
            return false;
        }

        return await _dbContext.Cases
            .AsNoTracking()
            .AnyAsync(caseEntity =>
                caseEntity.Id == caseId &&
                !caseEntity.IsDeleted &&
                caseEntity.DoctorClinicId == user.DoctorClinicId.Value,
                cancellationToken);
    }
}
