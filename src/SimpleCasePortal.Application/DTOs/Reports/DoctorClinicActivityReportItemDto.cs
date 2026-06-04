namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class DoctorClinicActivityReportItemDto
{
    public int DoctorClinicId { get; set; }

    public string DoctorClinicName { get; set; } = string.Empty;

    public int TotalCases { get; set; }

    public int UploadedFiles { get; set; }

    public int SuccessfulSyncCount { get; set; }

    public int FailedSyncCount { get; set; }

    public DateTime? LastCaseDate { get; set; }
}
