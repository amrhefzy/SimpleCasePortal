# Permission Matrix

This matrix documents the default production role permissions for Simple Case Portal.

| Permission | SuperAdmin | Admin | Doctor | Clinic | Viewer |
| --- | --- | --- | --- | --- | --- |
| Cases.Create | Yes | Yes | Yes | Yes | No |
| Cases.View.All | Yes | Yes | No | No | Yes |
| Cases.View.Own | Yes | Yes | Yes | Yes | No |
| Cases.Update | Yes | Yes | Yes | Yes | No |
| Cases.Delete.Soft | Yes | Yes | No | No | No |
| Files.Upload | Yes | Yes | Yes | Yes | No |
| Files.View | Yes | Yes | Yes | Yes | Yes |
| Files.Download | Yes | Yes | Yes | Yes | Yes |
| Files.Delete.Soft | Yes | Yes | Yes | Yes | No |
| Sync.Dentist | Yes | Yes | No | No | No |
| Sync.Workflow | Yes | Yes | No | No | No |
| Sync.Production | Yes | Yes | No | No | No |
| Sync.Retry | Yes | Yes | No | No | No |
| Users.Manage | Yes | Yes | No | No | No |
| Roles.Manage | Yes | Yes | No | No | No |
| Reports.View | Yes | Yes | No | No | No |
| Audit.View | Yes | Yes | No | No | No |
| Settings.Manage | Yes | No | No | No | No |

## Role Notes

- `SuperAdmin`: full system access.
- `Admin`: operational full access for cases, files, sync, reports, users, roles, and audit.
- `Doctor`: create/manage own cases and files only.
- `Clinic`: create/manage own cases and files only.
- `Viewer`: read-only case and file access. Viewer does not have `Reports.View` in the release seed.

## Ownership Rules

- Users with `Cases.View.All` can see all cases.
- Doctor/Clinic users without all-data permissions are scoped to their linked `DoctorClinicId`.
- Direct URL access must pass the same service-level ownership checks as navigation-driven access.
