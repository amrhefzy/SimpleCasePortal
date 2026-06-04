# Rollback Plan

Use this when a production deployment fails validation.

## Before Deployment

- Take a SQL Server backup.
- Save the current publish folder as a timestamped release, for example `/var/www/releases/casebridge-YYYYMMDD-HHMM`.
- Record the currently running migration from `__EFMigrationsHistory`.
- Confirm object storage is healthy before deploying.

## If App Startup Fails

1. Stop the service:

```bash
sudo systemctl stop casebridge
```

2. Repoint `/var/www/casebridge` to the previous publish folder, or restore the previous folder contents.
3. Start the service:

```bash
sudo systemctl start casebridge
```

4. Check:

```bash
sudo systemctl status casebridge
curl -I https://YOUR_PUBLIC_HOSTNAME/health
```

## If Migration Fails

1. Stop the application.
2. Restore the pre-migration SQL Server backup.
3. Restore the previous publish folder.
4. Start the application and confirm `/health`.
5. Review migration logs in a non-production environment before retrying.

## If File Storage Fails

1. Keep the app online only if case/file operations fail safely.
2. Verify `Storage__Provider=DigitalOceanSpaces`.
3. Verify service URL, region, bucket, access key, and secret key from the secret store.
4. Confirm the bucket is private and the credentials have required bucket actions.
5. Run a controlled upload/download test with a non-sensitive STL file after fixing configuration.

## After Rollback

- Document the failure time, release version, database backup used, and operator.
- Keep failed release artifacts for troubleshooting.
- Do not delete the pre-migration database backup until root cause is confirmed.
