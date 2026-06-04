using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Cases;

public sealed class CaseListFilterDto
{
    public string? CaseNumber { get; set; }

    public string? PatientName { get; set; }

    public CaseStatusEnum? Status { get; set; }

    public int? DoctorClinicId { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }
}
