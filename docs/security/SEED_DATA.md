# Seed Data

## Production Seeded Roles

- `SuperAdmin`
- `Admin`
- `Doctor`
- `Clinic`
- `Viewer`

## Production Seeded Permissions

- `Cases.Create`
- `Cases.View.All`
- `Cases.View.Own`
- `Cases.Update`
- `Cases.Delete.Soft`
- `Files.Upload`
- `Files.View`
- `Files.Download`
- `Files.Delete.Soft`
- `Sync.Dentist`
- `Sync.Workflow`
- `Sync.Production`
- `Sync.Retry`
- `Users.Manage`
- `Roles.Manage`
- `Reports.View`
- `Audit.View`
- `Settings.Manage`

## Production Role Permission Intent

The release permission matrix is documented in `docs/security/PERMISSION_MATRIX.md`.

Viewer is intentionally read-only for cases and files only. Viewer does not have `Reports.View` in the production seed.

## Development-Only Seed Users

Development users are created only when all of these are true:

- `ASPNETCORE_ENVIRONMENT=Development`
- `DevelopmentSeed:Enabled` is true or omitted
- `DevelopmentSeed:TemporaryPassword` is configured

Default development users:

- `superadmin@casebridge.local`
- `admin@casebridge.local`
- `doctor@casebridge.local`
- `clinic@casebridge.local`
- `viewer@casebridge.local`
- `inactive@casebridge.local`

The development temporary password is stored only in `appsettings.Development.json`, which is excluded from publish output.

## Development Sample Data

Development sample doctor/clinic records and sample cases are created only through `DevelopmentAuthSeeder` and only in Development. They are not part of production EF seed data.
