using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Web.Ui;

public static class UiFormatters
{
    public static string CaseStatusBadgeClass(CaseStatusEnum status)
    {
        return status switch
        {
            CaseStatusEnum.Draft => "text-bg-secondary",
            CaseStatusEnum.Submitted => "text-bg-primary",
            CaseStatusEnum.SyncedToDentist or CaseStatusEnum.SyncedToWorkflow or CaseStatusEnum.SyncedToProduction => "text-bg-success",
            CaseStatusEnum.SyncFailed => "text-bg-danger",
            CaseStatusEnum.Archived => "text-bg-dark",
            _ => "text-bg-secondary"
        };
    }

    public static string SyncStatusBadgeClass(SyncStatusEnum? status)
    {
        return status switch
        {
            SyncStatusEnum.Pending => "text-bg-warning",
            SyncStatusEnum.Success => "text-bg-success",
            SyncStatusEnum.Failed => "text-bg-danger",
            _ => "text-bg-secondary"
        };
    }

    public static string SyncTargetLabel(SyncTargetEnum target)
    {
        return target switch
        {
            SyncTargetEnum.DentistApp => "Dentist App",
            SyncTargetEnum.WorkflowApp => "Workflow App",
            SyncTargetEnum.ProductionApp => "Production App",
            _ => target.ToString()
        };
    }

    public static string FileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
        {
            return $"{bytes / 1024d / 1024d:0.##} MB";
        }

        if (bytes >= 1024)
        {
            return $"{bytes / 1024d:0.##} KB";
        }

        return $"{bytes} B";
    }
}
