namespace SimpleCasePortal.Application.DTOs.Sync;

public sealed class ExternalPatientDto
{
    public string Name { get; set; } = default!;

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }
}
