using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data;

internal static class SeedData
{
    internal const int SuperAdminRoleId = 1;
    internal const int AdminRoleId = 2;
    internal const int DoctorRoleId = 3;
    internal const int ClinicRoleId = 4;
    internal const int ViewerRoleId = 5;

    internal const int CasesCreatePermissionId = 1;
    internal const int CasesViewAllPermissionId = 2;
    internal const int CasesViewOwnPermissionId = 3;
    internal const int CasesUpdatePermissionId = 4;
    internal const int CasesDeleteSoftPermissionId = 5;
    internal const int FilesUploadPermissionId = 6;
    internal const int FilesViewPermissionId = 7;
    internal const int FilesDownloadPermissionId = 8;
    internal const int FilesDeleteSoftPermissionId = 9;
    internal const int SyncDentistPermissionId = 10;
    internal const int SyncWorkflowPermissionId = 11;
    internal const int SyncProductionPermissionId = 12;
    internal const int SyncRetryPermissionId = 13;
    internal const int UsersManagePermissionId = 14;
    internal const int RolesManagePermissionId = 15;
    internal const int ReportsViewPermissionId = 16;
    internal const int AuditViewPermissionId = 17;
    internal const int SettingsManagePermissionId = 18;

    internal static readonly Role[] Roles =
    [
        new() { Id = SuperAdminRoleId, Name = RoleNames.SuperAdmin, Description = "Full system access.", IsSystemRole = true },
        new() { Id = AdminRoleId, Name = RoleNames.Admin, Description = "Administrative access to cases, users, files, sync, reports, and audit.", IsSystemRole = true },
        new() { Id = DoctorRoleId, Name = RoleNames.Doctor, Description = "Doctor access to create and manage owned cases and files.", IsSystemRole = true },
        new() { Id = ClinicRoleId, Name = RoleNames.Clinic, Description = "Clinic access to create and manage owned cases and files.", IsSystemRole = true },
        new() { Id = ViewerRoleId, Name = RoleNames.Viewer, Description = "Read-only case and file access.", IsSystemRole = true }
    ];

    internal static readonly Permission[] Permissions =
    [
        Permission(CasesCreatePermissionId, PermissionNames.CasesCreate),
        Permission(CasesViewAllPermissionId, PermissionNames.CasesViewAll),
        Permission(CasesViewOwnPermissionId, PermissionNames.CasesViewOwn),
        Permission(CasesUpdatePermissionId, PermissionNames.CasesUpdate),
        Permission(CasesDeleteSoftPermissionId, PermissionNames.CasesDeleteSoft),
        Permission(FilesUploadPermissionId, PermissionNames.FilesUpload),
        Permission(FilesViewPermissionId, PermissionNames.FilesView),
        Permission(FilesDownloadPermissionId, PermissionNames.FilesDownload),
        Permission(FilesDeleteSoftPermissionId, PermissionNames.FilesDeleteSoft),
        Permission(SyncDentistPermissionId, PermissionNames.SyncDentist),
        Permission(SyncWorkflowPermissionId, PermissionNames.SyncWorkflow),
        Permission(SyncProductionPermissionId, PermissionNames.SyncProduction),
        Permission(SyncRetryPermissionId, PermissionNames.SyncRetry),
        Permission(UsersManagePermissionId, PermissionNames.UsersManage),
        Permission(RolesManagePermissionId, PermissionNames.RolesManage),
        Permission(ReportsViewPermissionId, PermissionNames.ReportsView),
        Permission(AuditViewPermissionId, PermissionNames.AuditView),
        Permission(SettingsManagePermissionId, PermissionNames.SettingsManage)
    ];

    internal static readonly RolePermission[] RolePermissions = BuildRolePermissions();

    private static Permission Permission(int id, string name)
    {
        return new Permission { Id = id, Name = name, Description = name };
    }

    private static RolePermission[] BuildRolePermissions()
    {
        var rolePermissions = new List<RolePermission>();

        Add(SuperAdminRoleId, Enumerable.Range(CasesCreatePermissionId, SettingsManagePermissionId));

        Add(AdminRoleId,
        [
            CasesCreatePermissionId,
            CasesViewAllPermissionId,
            CasesViewOwnPermissionId,
            CasesUpdatePermissionId,
            CasesDeleteSoftPermissionId,
            FilesUploadPermissionId,
            FilesViewPermissionId,
            FilesDownloadPermissionId,
            FilesDeleteSoftPermissionId,
            SyncDentistPermissionId,
            SyncWorkflowPermissionId,
            SyncProductionPermissionId,
            SyncRetryPermissionId,
            UsersManagePermissionId,
            RolesManagePermissionId,
            ReportsViewPermissionId,
            AuditViewPermissionId
        ]);

        Add(DoctorRoleId,
        [
            CasesCreatePermissionId,
            CasesViewOwnPermissionId,
            CasesUpdatePermissionId,
            FilesUploadPermissionId,
            FilesViewPermissionId,
            FilesDownloadPermissionId,
            FilesDeleteSoftPermissionId
        ]);

        Add(ClinicRoleId,
        [
            CasesCreatePermissionId,
            CasesViewOwnPermissionId,
            CasesUpdatePermissionId,
            FilesUploadPermissionId,
            FilesViewPermissionId,
            FilesDownloadPermissionId,
            FilesDeleteSoftPermissionId
        ]);

        Add(ViewerRoleId,
        [
            CasesViewAllPermissionId,
            FilesViewPermissionId,
            FilesDownloadPermissionId
        ]);

        return [.. rolePermissions];

        void Add(int roleId, IEnumerable<int> permissionIds)
        {
            rolePermissions.AddRange(permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            }));
        }
    }
}
