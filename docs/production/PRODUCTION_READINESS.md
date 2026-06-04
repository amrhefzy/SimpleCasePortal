# Production Readiness

This guide prepares Simple Case Portal for production deployment without storing secrets in source control.

## Required Environment Variables

Use environment variables, a systemd `EnvironmentFile`, or a secret manager. Do not edit committed JSON files with real values.

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://127.0.0.1:5000`
- `ConnectionStrings__DefaultConnection`
- `Storage__Provider=DigitalOceanSpaces`
- `Storage__ServiceUrl`
- `Storage__Region`
- `Storage__BucketName`
- `Storage__AccessKey`
- `Storage__SecretKey`
- `Storage__SignedUrlExpiryMinutes`
- `Storage__MaxFileSizeMb`
- `ExternalApis__UseFakeClientsInDevelopment=false`
- `ExternalApis__DentistApp__BaseUrl`
- `ExternalApis__DentistApp__Endpoint`
- `ExternalApis__DentistApp__ApiKey`
- `ExternalApis__DentistApp__TimeoutSeconds`
- `ExternalApis__DentistApp__SendSignedFileUrls`
- `ExternalApis__WorkflowApp__BaseUrl`
- `ExternalApis__WorkflowApp__Endpoint`
- `ExternalApis__WorkflowApp__ApiKey`
- `ExternalApis__WorkflowApp__TimeoutSeconds`
- `ExternalApis__WorkflowApp__SendSignedFileUrls`
- `ExternalApis__ProductionApp__BaseUrl`
- `ExternalApis__ProductionApp__Endpoint`
- `ExternalApis__ProductionApp__ApiKey`
- `ExternalApis__ProductionApp__TimeoutSeconds`
- `ExternalApis__ProductionApp__SendSignedFileUrls`
- `DataProtection__KeysPath`
- `AllowedHosts`

`Storage__Provider=LocalDevelopment` is only for local development. Production must use private object storage.

## Build And Publish

Run from the repository root:

```bash
dotnet build SimpleCasePortal.sln
dotnet publish src/SimpleCasePortal.Web/SimpleCasePortal.Web.csproj -c Release -o ./publish
```

Copy the publish output to the server folder, for example `/var/www/casebridge`. The publish folder must not contain local storage files, Data Protection keys, source-only docs, or real secrets.

## Server Folders

- App folder: `/var/www/casebridge`
- Environment file folder: `/etc/casebridge`
- Data Protection keys: `/var/lib/casebridge/dp-keys`
- Operational backups: `/var/backups/casebridge`
- Application logs, if file logging is added later: `/var/log/casebridge`

The app user must be able to read the app folder and environment file, and read/write the Data Protection key folder. Do not grant write access to `wwwroot` for uploads.

## SQL Server Migrations

Back up the database before every migration.

```bash
dotnet ef database update --project src/SimpleCasePortal.Infrastructure --startup-project src/SimpleCasePortal.Web
```

After migration, confirm the app connects and `__EFMigrationsHistory` contains the expected latest migration. If a migration fails, stop the app, restore the database backup, restore the previous app publish folder, and review logs before retrying.

## DigitalOcean Spaces

- Use a private bucket only.
- Do not enable public object access or `public-read`.
- Use least-privilege access keys scoped to the required bucket actions.
- Configure service URL, region, bucket, access key, and secret key through environment variables.
- Signed URL expiry is controlled by `Storage__SignedUrlExpiryMinutes`.
- The app stores object keys only. It generates temporary signed download URLs on demand.
- Signed URLs and raw object keys must not be stored in logs.
- Enable bucket versioning or object-lock features where available.
- Keep provider access logs if available.

Deployment validation should include one upload/download test using a non-sensitive STL test file.

## Retention And Backups

Clinical case files may need retention up to 10 years.

- Run SQL Server backups at least daily.
- Keep off-server backups and periodically test restores.
- Align object storage lifecycle/versioning with the retention policy.
- Soft delete is the application-level removal model; physical deletion should require an operational approval process.
- Decide audit log retention explicitly with legal/compliance stakeholders.
- Monitor database growth, object storage growth, and monthly egress. Model cost at 1 TB and 5 TB usage before go-live if large STL volume is expected.

## Reverse Proxy

Run behind Nginx or equivalent HTTPS reverse proxy. Forward `Host`, `X-Real-IP`, `X-Forwarded-For`, and `X-Forwarded-Proto`. Set `client_max_body_size` to at least the configured max upload size, for example `250M`.

## HTTPS

Install TLS certificates, enable automatic renewal, redirect HTTP to HTTPS, and confirm HSTS is active in production.

## Data Protection Keys

Data Protection keys protect auth cookies and local token signing. Persist keys outside the publish folder, for example `/var/lib/casebridge/dp-keys`, and protect that folder with filesystem permissions. If keys are lost, active authentication cookies become invalid.

## Audit Review

- Audit logs are read-only in the application UI.
- Access requires `Audit.View`.
- Sensitive values are masked at display time.
- Review login failures, file activity, soft deletes, and sync failures periodically.

## Final Production Readiness Checklist

- Build passes.
- Publish passes.
- Production environment variables are configured.
- SQL Server backup is completed before migration.
- Migrations are applied and `__EFMigrationsHistory` is updated.
- Data Protection keys folder exists and is writable by the app user.
- App folder permissions are correct.
- `Storage__Provider=DigitalOceanSpaces`.
- `LocalDevelopment` storage is disabled in Production.
- DigitalOcean Spaces credentials are configured through secrets/environment.
- Bucket is private.
- Upload/download test completed.
- Nginx `client_max_body_size` supports STL uploads.
- HTTPS is enabled and certificates renew automatically.
- Firewall exposes only required ports.
- Backups and restore tests are scheduled.
- `/health` returns `200`.
- Login works.
- Case create works.
- STL upload works.
- Signed download works.
- Manual sync works with real targets, or fails safely if a target is not configured.
- Fake external clients are disabled in Production.
- Audit logs work.
- No secrets are present in the repository or publish folder.
