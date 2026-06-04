namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class CaseSummaryReportDto
{
    public int TotalCases { get; set; }

    public int DraftCases { get; set; }

    public int SubmittedCases { get; set; }

    public int SyncedCases { get; set; }

    public int FailedSyncCases { get; set; }

    public int ArchivedCases { get; set; }
}
