# Deployment Checklist

## Local Build And Publish

```bash
dotnet build SimpleCasePortal.sln
dotnet publish src/SimpleCasePortal.Web/SimpleCasePortal.Web.csproj -c Release -o ./publish
```

Confirm:

- Publish output contains `SimpleCasePortal.Web.dll`.
- Publish output does not contain `App_Data/LocalStorage`.
- Publish output does not contain `App_Data/DataProtectionKeys`.
- Publish output does not contain real secrets.
- Production can run without `appsettings.Development.json`.

## Server Install

Example target paths:

- App: `/var/www/casebridge`
- Environment file: `/etc/casebridge/casebridge.env`
- Data Protection keys: `/var/lib/casebridge/dp-keys`
- Backups: `/var/backups/casebridge`
- Logs: `/var/log/casebridge`

Create an app user such as `www-data` or an app-specific user. Grant read access to the app folder and read/write access to the Data Protection folder. Do not grant write access to `wwwroot` for uploaded files.

## Database

1. Back up SQL Server.
2. Run:

```bash
dotnet ef database update --project src/SimpleCasePortal.Infrastructure --startup-project src/SimpleCasePortal.Web
```

3. Confirm app startup and database connectivity.
4. Confirm `__EFMigrationsHistory` contains the expected latest migration.

## systemd

Use `docs/production/SYSTEMD_SERVICE_SAMPLE.service` as a template. Keep secrets in `/etc/casebridge/casebridge.env`, not inline in the service unit.

## Nginx

Use `docs/production/NGINX_SAMPLE.conf` as a template. Confirm:

- `proxy_pass` targets the app URL from `ASPNETCORE_URLS`.
- Forwarded headers are present.
- `client_max_body_size` is at least the configured max upload size.
- HTTPS is enabled with Certbot or equivalent.

## Firewall

- Allow SSH only from trusted sources where possible.
- Allow HTTP/HTTPS for the public site.
- Keep SQL Server restricted to trusted networks.
- Do not expose internal app port `5000` publicly.

## Backups

- SQL Server daily backups.
- Off-server backup copy.
- Object storage versioning or equivalent protection.
- Periodic restore test.
- Document retention and physical-delete approval process.

## Rollback

Use `docs/production/ROLLBACK_PLAN.md`. Keep the previous app publish folder and the pre-migration database backup until the deployment is validated.

## Final Checks

- `ASPNETCORE_ENVIRONMENT=Production`.
- `Storage__Provider=DigitalOceanSpaces`.
- Development seed users disabled.
- Fake external API clients disabled.
- `/health` returns `200`.
- Login/logout work.
- Case create works.
- STL upload/download work.
- Manual sync works or fails safely for unconfigured targets.
- Audit Logs are accessible only to `Audit.View` users.
- No public file URLs are introduced.
- No signed URLs are stored in DB/logs.
