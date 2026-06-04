# Release Notes v0.1

## Completed Modules

- Layered ASP.NET Core MVC solution.
- SQL Server EF Core domain/database foundation.
- Cookie authentication with secure password hashing.
- Role and permission authorization.
- Doctor/Clinic ownership scoping.
- Case create/edit/details/list/soft-delete.
- Secure file upload, signed download, and soft-delete.
- Manual external sync with retry and failure tracking.
- Audit logging and audit UI.
- Dashboard and UI polish.
- Security hardening and production readiness docs.
- Production deployment samples for Nginx, systemd, environment variables, rollback, and readiness checks.
- Operational reports for case volume, file activity, sync activity, and failed sync follow-up.

## Security Model Summary

- Permissions are stored in role/permission tables.
- Controllers use authorization policies and application services.
- Ownership checks are enforced server-side.
- Business entities use soft delete.
- File records store object keys only.
- Downloads use short-lived signed URLs.
- Public file URLs are not used.
- Sync logs redact storage locators and signed URLs.
- Production error handling avoids stack trace exposure.

## Deployment Requirements

- SQL Server database.
- Production connection string supplied via environment/secret store.
- Private DigitalOcean Spaces bucket and least-privilege credentials.
- Persistent Data Protection key folder outside the publish directory.
- HTTPS reverse proxy such as Nginx.
- `ASPNETCORE_ENVIRONMENT=Production`.
- Production migration execution after database backup.
- Backup and restore process for SQL Server and object storage.

## Known Limitations

- DigitalOcean Spaces has not been live-tested without real credentials.
- External APIs have not been live-tested without real endpoints.
- Development fake clients are used only in Development.
- Production deployment has not yet been performed from this workspace.
- CSV export for reports is not included in v0.1.
- User/role management UI is not included yet.

## Pending Live Tests

- Production SQL Server migration against a real production database.
- Private bucket upload/download test with DigitalOcean Spaces.
- Real Dentist, Workflow, and Production API sync tests.
- HTTPS reverse proxy test using the production hostname.
- Restore test from SQL Server backup and object storage backup/versioning.
