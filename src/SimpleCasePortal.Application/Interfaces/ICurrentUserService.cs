namespace SimpleCasePortal.Application.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? FullName { get; }

    string? Email { get; }

    int? DoctorClinicId { get; }

    bool IsInRole(string roleName);
}
