# Final Production Readiness Checklist

Use this immediately before go-live.

## Build And Publish

- `dotnet build SimpleCasePortal.sln` succeeds.
- `dotnet publish src/SimpleCasePortal.Web/SimpleCasePortal.Web.csproj -c Release -o ./publish` succeeds.
- Publish output contains `SimpleCasePortal.Web.dll`.
- Publish output does not include `App_Data/LocalStorage`.
- Publish output does not include Data Protection keys.
- Publish output does not include real secrets.

## Configuration

- `ASPNETCORE_ENVIRONMENT=Production`.
- `ASPNETCORE_URLS` matches the Nginx `proxy_pass` target.
- `AllowedHosts` is set to the public hostname.
- `ConnectionStrings__DefaultConnection` points to production SQL Server.
- `Storage__Provider=DigitalOceanSpaces`.
- `LocalDevelopment` storage is not used in Production.
- `ExternalApis__UseFakeClientsInDevelopment=false`.
- All external API base URLs and API keys are configured through secrets/environment.
- `DataProtection__KeysPath` points outside the publish folder.

## Server

- App folder permissions are read-only for application runtime except required runtime files.
- Data Protection keys folder exists and is writable by the app user.
- Backup folder exists and is restricted.
- Log folder exists if file logging is configured.
- systemd service uses `EnvironmentFile`.
- Nginx forwards `Host`, `X-Real-IP`, `X-Forwarded-For`, and `X-Forwarded-Proto`.
- Nginx `client_max_body_size` supports STL uploads.
- HTTPS is enabled and certificate renewal is configured.
- Firewall exposes only required ports.

## Database

- SQL Server backup completed before migration.
- Migration command completed successfully.
- `__EFMigrationsHistory` shows the expected latest migration.
- Application connects to the database after deployment.

## Storage

- DigitalOcean Spaces bucket is private.
- No public-read policy is enabled.
- Storage credentials have least-privilege access.
- Signed URL expiry is configured.
- Upload test completed with a non-sensitive STL file.
- Signed download test completed.
- No signed URLs are stored in DB/logs.

## Application Smoke Test

- `/health` returns `200`.
- Login works.
- Logout works.
- Case create works.
- STL upload works.
- Signed download works.
- Manual sync works or fails safely for missing target configuration.
- Audit logs work and are only visible to authorized users.
- Unauthorized direct URLs redirect or forbid access.

## Operations

- SQL Server daily backups are scheduled.
- Off-server backups are configured.
- Object storage versioning or retention controls are configured where available.
- Restore test is scheduled.
- Audit log retention decision is documented.
- Soft delete vs physical delete process is documented.
- Storage growth and egress cost monitoring is configured.
