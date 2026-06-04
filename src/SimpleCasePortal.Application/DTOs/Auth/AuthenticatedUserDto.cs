namespace SimpleCasePortal.Application.DTOs.Auth;

public sealed class AuthenticatedUserDto
{
    public string Id { get; set; } = default!;

    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public int? DoctorClinicId { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; } = [];
}
