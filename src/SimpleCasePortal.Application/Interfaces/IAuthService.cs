using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Auth;

namespace SimpleCasePortal.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthenticatedUserDto>> ValidateCredentialsAsync(
        LoginRequestDto request,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}
