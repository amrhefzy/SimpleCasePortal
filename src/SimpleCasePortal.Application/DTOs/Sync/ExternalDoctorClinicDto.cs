namespace SimpleCasePortal.Application.DTOs.Sync;

public sealed class ExternalDoctorClinicDto
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Email { get; set; }
}
