using SimpleCasePortal.Domain.Constants;

namespace SimpleCasePortal.Web.Authorization;

public static class PermissionPolicyNames
{
    public static readonly string[] AllPermissions =
    [
        PermissionNames.CasesCreate,
        PermissionNames.CasesViewAll,
        PermissionNames.CasesViewOwn,
        PermissionNames.CasesUpdate,
        PermissionNames.CasesDeleteSoft,
        PermissionNames.FilesUpload,
        PermissionNames.FilesView,
        PermissionNames.FilesDownload,
        PermissionNames.FilesDeleteSoft,
        PermissionNames.SyncDentist,
        PermissionNames.SyncWorkflow,
        PermissionNames.SyncProduction,
        PermissionNames.SyncRetry,
        PermissionNames.UsersManage,
        PermissionNames.RolesManage,
        PermissionNames.ReportsView,
        PermissionNames.AuditView,
        PermissionNames.SettingsManage
    ];

    public static string For(string permissionName)
    {
        return $"Permission:{permissionName}";
    }
}
