using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Dashboard;

public sealed class DashboardCaseDto
{
    public int Id { get; set; }

    public string CaseNumber { get; set; } = default!;

    public string PatientName { get; set; } = default!;

    public string DoctorClinicName { get; set; } = default!;

    public CaseStatusEnum Status { get; set; }

    public DateTime CreatedOn { get; set; }
}
