using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Application.DTOs.Reports;

public sealed class CaseStatusReportItemDto
{
    public CaseStatusEnum Status { get; set; }

    public int Count { get; set; }

    public decimal Percentage { get; set; }
}
