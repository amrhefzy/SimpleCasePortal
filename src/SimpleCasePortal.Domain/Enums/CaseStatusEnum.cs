namespace SimpleCasePortal.Domain.Enums;

public enum CaseStatusEnum
{
    Draft = 1,
    Submitted = 2,
    SyncedToDentist = 3,
    SyncedToWorkflow = 4,
    SyncedToProduction = 5,
    SyncFailed = 6,
    Archived = 7
}
