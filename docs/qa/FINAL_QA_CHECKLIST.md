# Final QA Checklist

Run this checklist before the first release.

## Authentication

- Login succeeds with valid credentials.
- Login fails with invalid password.
- Inactive user cannot log in.
- Logout is POST-only and uses anti-forgery.

## Authorization

- Admin can see all cases.
- Doctor can see only own cases.
- Clinic can see only own cases.
- Viewer has read-only case/file behavior.
- Direct URLs are blocked when permissions or ownership do not allow access.

## Cases

- Create case.
- Edit case.
- View details.
- Soft delete case.
- Confirm case number generation format and uniqueness.

## Files

- Upload STL file.
- Reject invalid extension.
- Reject dangerous extension.
- Download through signed URL.
- Soft delete file.
- Confirm deleted file cannot download.
- Confirm no uploaded file is saved in `wwwroot`.

## Sync

- Sync Dentist.
- Sync Workflow.
- Sync Production.
- Failed sync is recorded safely.
- Retry failed sync.
- Confirm sync endpoints are POST-only.

## Audit

- Login success/failure audit.
- Case audit.
- File audit.
- Sync audit.
- Confirm sensitive masking.

## Reports

- `Reports.View` is required.
- Admin can open reports.
- Viewer is blocked from reports in the release permission matrix.
- Doctor/Clinic reports are scoped to own data if `Reports.View` is granted later.
- Reports do not show object keys, signed URLs, raw sync payload JSON, or secrets.

## Production

- `dotnet build SimpleCasePortal.sln` succeeds.
- `dotnet publish src/SimpleCasePortal.Web/SimpleCasePortal.Web.csproj -c Release -o ./publish` succeeds.
- `/health` returns `200`.
- Security headers are present.
- No secrets in source or publish output.
- No public file URLs.
- Production uses private object storage.

## Regression

- Dashboard loads.
- Cases list/details load.
- File upload/download/delete still work.
- Manual sync still works or fails safely if configuration is missing.
- Audit UI loads for authorized users.
- Reports load for authorized users.
- No controller directly injects `DbContext`.
