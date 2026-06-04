# Implementation Tasks for Codex

## Phase 1 - Solution Setup

1. Create ASP.NET Core MVC solution.
2. Add projects:
   - Web
   - Application
   - Domain
   - Infrastructure
3. Add project references.
4. Configure SQL Server connection.
5. Configure EF Core.
6. Add dependency injection for services and repositories.
7. Add base `ApiResponse<T>` class.

Acceptance Criteria:

- Solution builds successfully.
- Web app runs.
- Layers are connected correctly.
- No business logic in controllers.

---

## Phase 2 - Domain and Database

1. Create entities:
   - Case
   - CaseFile
   - DoctorClinic
   - CaseSyncLog
   - AuditLog
   - Role
   - Permission
   - RolePermission
2. Create enums.
3. Create DbContext.
4. Add EF configurations.
5. Add migrations.
6. Seed default roles and permissions.

Acceptance Criteria:

- Database is created successfully.
- Tables match blueprint.
- Default roles and permissions are inserted.

---

## Phase 3 - Authentication and Authorization

1. Implement login.
2. Implement user roles.
3. Implement permission checks.
4. Protect controllers.
5. Ensure doctors/clinics can access only own cases.
6. Add admin access rules.

Acceptance Criteria:

- Unauthorized users cannot access protected pages.
- Doctor sees only own cases.
- Admin sees all cases.
- Permission checks work for sync and file download.

---

## Phase 4 - Case Management

1. Create case list screen.
2. Add create case screen.
3. Generate automatic case number.
4. Add edit case screen.
5. Add case details screen.
6. Add status display.
7. Add filters by case number, patient, doctor/clinic, status, date.
8. Add soft delete.

Acceptance Criteria:

- User can create case.
- Case number is unique and generated server-side.
- Case details show patient and file sections.
- Soft deleted cases are hidden by default.

---

## Phase 5 - File Upload

1. Add DigitalOcean Spaces options.
2. Implement `IFileStorageService`.
3. Implement upload to private bucket.
4. Add file validation.
5. Add safe filename generation.
6. Save file metadata.
7. Generate signed download URL.
8. Display files in case details.
9. Add soft delete for files.

Acceptance Criteria:

- STL files upload successfully.
- Files are not saved in `wwwroot`.
- DB stores object key, not public URL.
- Download uses signed URL.
- User cannot download files without permission.

---

## Phase 6 - Manual API Sync

1. Create external API options.
2. Implement API clients:
   - DentistApiClient
   - WorkflowApiClient
   - ProductionApiClient
3. Implement `ExternalSyncService`.
4. Add sync buttons in case details.
5. Add permission checks.
6. Log request and response.
7. Display sync status.
8. Add retry failed sync.

Acceptance Criteria:

- Sync button sends payload.
- Sync logs are saved.
- Errors are visible to admin.
- Unauthorized users cannot sync.

---

## Phase 7 - Audit Logging

1. Implement AuditService.
2. Log important actions.
3. Add Audit screen for admins.
4. Add filters by user, action, entity, date.

Acceptance Criteria:

- Case creation is logged.
- File upload/download/delete is logged.
- Sync success/failure is logged.
- Admin can view logs.

---

## Phase 8 - UI / UX

1. Clean dashboard.
2. Case cards or table.
3. Clear upload section.
4. Clear sync status section.
5. Validation messages.
6. Loading state for upload and sync buttons.
7. Responsive layout.

Acceptance Criteria:

- UI is clean and practical.
- User understands case status and sync status.
- Upload errors are clear.

---

## Phase 9 - Security Hardening

1. Validate all inputs.
2. Add anti-forgery tokens.
3. Enforce HTTPS.
4. Add secure cookie settings.
5. Confirm storage is private.
6. Confirm signed URL expiry.
7. Confirm no secrets in repo.
8. Add error handling.
9. Add backup instructions.

Acceptance Criteria:

- Security requirements document is satisfied.
- Direct file public access is not possible.
- App does not expose stack traces in production.

---

## Phase 10 - Deployment Preparation

1. Prepare production appsettings template.
2. Add Nginx deployment notes.
3. Add systemd service example.
4. Add database migration command.
5. Add backup plan.
6. Add environment variable documentation.

Acceptance Criteria:

- App can be deployed to VPS.
- Required settings are documented.
- Deployment is repeatable.
