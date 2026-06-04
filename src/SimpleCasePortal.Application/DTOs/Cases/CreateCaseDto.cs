namespace SimpleCasePortal.Application.DTOs.Cases;

public sealed class CreateCaseDto
{
    public int? DoctorClinicId { get; set; }

    public string PatientName { get; set; } = default!;

    public int? Age { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Notes { get; set; }

    public string CreatedByUserId { get; set; } = default!;
}
