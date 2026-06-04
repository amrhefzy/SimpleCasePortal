# Codex Master Prompt - Simple Case Portal

You are a senior .NET architect and senior ASP.NET Core MVC developer.

I want you to implement a professional but simple application called **Simple Case Portal**.

The application is similar in concept to our Dentist App, Workflow App, and Production App, but the first version must stay lightweight and practical.

## Main Objective

Build an ASP.NET Core MVC application that allows doctors or clinics to:

1. Login securely.
2. Create a new case.
3. Automatically generate a case number.
4. Enter patient information.
5. Upload STL files and related attachments.
6. Store files securely in DigitalOcean Spaces or S3-compatible storage.
7. Manually sync selected cases to external systems by clicking buttons:
   - Sync to Dentist App
   - Sync to Workflow App
   - Sync to Production App

The system must be clean, secure, and suitable for long-term clinical file retention up to 10 years.

---

## Very Important Development Rules

- Do not build quick hacks.
- Follow clean layered architecture.
- Use ASP.NET Core MVC.
- Use SQL Server.
- Use Entity Framework Core Code First.
- Use repositories and services.
- Controllers must not contain business logic.
- Use async methods for I/O.
- Use `ApiResponse<T>` for service responses.
- Use soft delete for business records.
- Never store uploaded files in `wwwroot`.
- Never expose permanent public file URLs.
- Store object keys in the database.
- Generate temporary signed URLs for viewing/downloading files.
- Do not hardcode secrets.
- Use secure configuration for connection strings, API keys, and storage keys.
- Add audit logs for important actions.
- Add permission checks for sensitive endpoints.
- Prefer complete and correct implementation over shortcuts.

---

## Required Architecture

Create or adapt the solution to use this structure:

```txt
SimpleCasePortal.Web
SimpleCasePortal.Application
SimpleCasePortal.Domain
SimpleCasePortal.Infrastructure
```

### Web Layer
MVC controllers, Razor Views, ViewModels, UI only.

### Application Layer
Services, DTOs, interfaces, business logic, validation, ApiResponse.

### Domain Layer
Entities, enums, constants.

### Infrastructure Layer
DbContext, EF Core repositories, DigitalOcean/S3 storage service, external API clients.

---

## Required Modules

### 1. Case Management

Features:

- Create case
- Edit case
- View case details
- List/search/filter cases
- Soft delete case
- Automatic case number

Required patient fields:

- PatientName
- Age
- DateOfBirth
- Gender
- Notes

Required case fields:

- Id
- CaseNumber
- DoctorClinicId
- Status
- CreatedOn
- UpdatedOn
- IsDeleted

Case number format:

```txt
CP-YYYY-000001
```

Example:

```txt
CP-2026-000001
```

Case number must be generated server-side only.

---

### 2. File Upload

The user must be able to upload files per case.

Initial allowed file types:

```txt
.stl
.jpg
.jpeg
.png
.pdf
.zip
```

Important:

- STL files are the main required files.
- Files must be uploaded to private DigitalOcean Spaces / S3-compatible storage.
- Database must store metadata and object key only.
- Signed URLs must be generated for temporary access.
- File deletion must be soft delete in the database.
- Optional future physical deletion must be SuperAdmin only.

File metadata required:

- CaseId
- FileType
- OriginalFileName
- StoredFileName
- ObjectKey
- ContentType
- FileExtension
- FileSizeBytes
- UploadedByUserId
- UploadedOn
- IsDeleted

Object key format:

```txt
cases/{caseNumber}/{fileType}/{yyyy}/{MM}/{uniqueFileId}_{safeFileName}
```

---

### 3. Users, Roles, and Permissions

Required roles:

- SuperAdmin
- Admin
- Doctor
- Clinic
- Viewer

Use permissions in addition to roles.

Required permissions:

```txt
Cases.Create
Cases.View.All
Cases.View.Own
Cases.Update
Cases.Delete.Soft

Files.Upload
Files.View
Files.Download
Files.Delete.Soft

Sync.Dentist
Sync.Workflow
Sync.Production
Sync.Retry

Users.Manage
Roles.Manage
Reports.View
Audit.View
Settings.Manage
```

Rules:

- Doctor and Clinic users can only see their own cases.
- Admin and SuperAdmin can see all cases.
- Sync buttons appear only when the user has permission.
- Download/view file actions require permission and ownership check.

---

### 4. Manual API Sync

In Case Details page, add three buttons:

```txt
Sync to Dentist App
Sync to Workflow App
Sync to Production App
```

Each button should:

1. Validate user permission.
2. Load case data and files.
3. Build target-specific payload.
4. Send HTTP request to configured endpoint.
5. Save sync log with request, response, status, error, and timestamp.
6. Update UI with sync result.

Create:

- `IExternalSyncService`
- `DentistApiClient`
- `WorkflowApiClient`
- `ProductionApiClient`

Required sync log fields:

- CaseId
- SyncTarget
- SyncStatus
- RequestPayload
- ResponsePayload
- ErrorMessage
- ExternalReferenceId
- SyncedByUserId
- SyncedOn
- RetryCount

---

### 5. Audit Logging

Add audit logs for:

- Login success/failure
- Case created
- Case updated
- Case deleted
- File uploaded
- File downloaded
- File deleted
- API sync started
- API sync succeeded
- API sync failed
- User created
- User role changed
- Permission changed

Audit log fields:

- UserId
- Action
- EntityName
- EntityId
- OldValues
- NewValues
- IpAddress
- UserAgent
- CreatedOn

---

### 6. Security Requirements

This system may store STL files and clinical case data for up to 10 years.

Mandatory security requirements:

- HTTPS only.
- Private object storage only.
- No permanent public links.
- Signed URLs with short expiry.
- Role and permission checks.
- Ownership checks.
- Soft delete.
- Audit logging.
- Secure cookies.
- Anti-forgery tokens.
- File extension validation.
- File size validation.
- Safe file name generation.
- Secrets outside source code.
- Backup strategy.
- Least privilege access keys.
- Error handling without exposing stack traces in production.

---

## Cost Information

Development estimate:

```txt
Total: $2,800
Hourly rate: $40/hour
Estimated hours: 70 hours
```

Running cost:

```txt
Server deployment: €600/year
Storage: $600/year for 1TB storage and 5TB traffic
```

---

## Implementation Method

Before writing code:

1. Inspect the existing repository structure.
2. Identify whether there is already a layered architecture.
3. Reuse existing patterns if present.
4. If no project exists, create the proposed solution structure.
5. Implement incrementally in small commits.
6. Build after each major step.
7. Do not leave broken code.

Recommended implementation order:

1. Solution structure
2. Domain entities and enums
3. DbContext and repositories
4. Services and ApiResponse
5. User/role/permission setup
6. Case CRUD
7. File upload to DigitalOcean/S3
8. Signed URL download
9. Manual API sync
10. Audit logging
11. UI polish
12. Security hardening
13. Deployment preparation

---

## Expected Output from You

When you work on this project:

- Explain what you changed.
- List files added/modified.
- Mention database migration names.
- Mention configuration keys needed.
- Mention any assumptions.
- Run build/test where possible.
- Do not skip security requirements.

Use the files in `/docs/codex/` as the project memory and implementation contract.
