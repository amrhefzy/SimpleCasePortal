namespace SimpleCasePortal.Application.DTOs.Sync;

public sealed class ExternalCaseSyncPayloadDto
{
    public string CaseNumber { get; set; } = default!;

    public ExternalPatientDto Patient { get; set; } = new();

    public ExternalDoctorClinicDto DoctorClinic { get; set; } = new();

    public IReadOnlyCollection<ExternalCaseFileDto> Files { get; set; } = [];

    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }
}
