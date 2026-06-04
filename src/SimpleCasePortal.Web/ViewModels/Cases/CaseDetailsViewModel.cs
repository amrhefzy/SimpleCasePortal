using SimpleCasePortal.Application.DTOs.Cases;
using SimpleCasePortal.Web.ViewModels.Files;
using SimpleCasePortal.Web.ViewModels.Sync;

namespace SimpleCasePortal.Web.ViewModels.Cases;

public sealed class CaseDetailsViewModel
{
    public CaseDto Case { get; set; } = default!;

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool CanUploadFiles { get; set; }

    public bool CanDownloadFiles { get; set; }

    public bool CanDeleteFiles { get; set; }

    public bool CanSyncDentist { get; set; }

    public bool CanSyncWorkflow { get; set; }

    public bool CanSyncProduction { get; set; }

    public bool CanRetrySync { get; set; }

    public IReadOnlyCollection<CaseFileViewModel> Files { get; set; } = [];

    public IReadOnlyCollection<CaseSyncStatusViewModel> SyncStatuses { get; set; } = [];

    public UploadCaseFileViewModel UploadFile { get; set; } = new();
}
