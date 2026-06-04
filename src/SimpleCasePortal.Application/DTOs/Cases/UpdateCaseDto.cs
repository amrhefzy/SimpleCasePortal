using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Cases;

public sealed class UpdateCaseDto
{
    public int Id { get; set; }

    public int? DoctorClinicId { get; set; }

    public string PatientName { get; set; } = default!;

    public int? Age { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Notes { get; set; }

    public CaseStatusEnum? Status { get; set; }

    public string UpdatedByUserId { get; set; } = default!;
}
