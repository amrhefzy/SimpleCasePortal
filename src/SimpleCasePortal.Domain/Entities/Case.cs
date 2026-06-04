using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Domain.Entities;

public sealed class Case
{
    public int Id { get; set; }

    public string CaseNumber { get; set; } = default!;

    public int DoctorClinicId { get; set; }

    public string PatientName { get; set; } = default!;

    public int? Age { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Notes { get; set; }

    public CaseStatusEnum Status { get; set; } = CaseStatusEnum.Draft;

    public string CreatedByUserId { get; set; } = default!;

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DoctorClinic DoctorClinic { get; set; } = default!;

    public ICollection<CaseFile> Files { get; set; } = [];

    public ICollection<CaseSyncLog> SyncLogs { get; set; } = [];
}
