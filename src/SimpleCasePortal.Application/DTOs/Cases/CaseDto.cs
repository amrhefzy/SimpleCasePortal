using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Cases;

public sealed class CaseDto
{
    public int Id { get; set; }

    public string CaseNumber { get; set; } = default!;

    public int DoctorClinicId { get; set; }

    public string DoctorClinicName { get; set; } = default!;

    public string PatientName { get; set; } = default!;

    public int? Age { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Notes { get; set; }

    public CaseStatusEnum Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
