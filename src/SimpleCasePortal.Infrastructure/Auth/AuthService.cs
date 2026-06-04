using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Auth;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Application.Security;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public AuthService(AppDbContext dbContext, IPasswordHasher passwordHasher, IAuditService auditService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    public async Task<ApiResponse<AuthenticatedUserDto>> ValidateCredentialsAsync(
        LoginRequestDto request,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Include(applicationUser => applicationUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(applicationUser => applicationUser.Email == email, cancellationToken);

        if (user is null || user.IsDeleted || !user.IsActive || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _auditService.LogAsync(
                AuditActionNames.LoginFailure,
                "ApplicationUser",
                email,
                user?.Id,
                ipAddress: ipAddress,
                userAgent: userAgent,
                cancellationToken: cancellationToken);

            return ApiResponse<AuthenticatedUserDto>.Fail("Invalid email or password.");
        }

        await _auditService.LogAsync(
            AuditActionNames.LoginSuccess,
            "ApplicationUser",
            user.Id,
            user.Id,
            ipAddress: ipAddress,
            userAgent: userAgent,
            cancellationToken: cancellationToken);

        return ApiResponse<AuthenticatedUserDto>.Ok(new AuthenticatedUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            DoctorClinicId = user.DoctorClinicId,
            Roles = user.UserRoles.Select(userRole => userRole.Role.Name).Order().ToArray()
        });
    }
}
