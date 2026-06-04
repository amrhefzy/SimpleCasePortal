# Project Memory Tree - Simple Case Portal

## 1. Product Identity

### Application Name
Simple Case Portal

### Business Type
Dental / aligner case intake and STL file upload portal.

### Main Business Goal
Create a lightweight but professional application that allows doctors or clinics to create cases, upload STL files, and manually sync the case to existing systems such as Dentist App, Workflow App, and Production App.

### Long-Term Vision
The application should start simple but be designed as a stable foundation for future expansion into:

- Case workflow tracking
- Production routing
- Doctor portal
- Treatment plan uploads
- Notifications
- Advanced file viewer
- Reporting
- Accounting integration
- AI / 3D modules later if required

---

## 2. Core Business Flow

1. Admin creates or approves a doctor / clinic account.
2. Doctor or clinic user logs in.
3. User creates a new case.
4. System generates automatic case number.
5. User enters patient information.
6. User uploads STL files and related case files.
7. Files are stored privately in object storage.
8. Database stores metadata and object keys only.
9. Admin or authorized user reviews the case.
10. User manually clicks sync button:
    - Send to Dentist App
    - Send to Workflow App
    - Send to Production App
11. System records sync status, response, error message, and timestamp.
12. Case remains searchable and auditable for long-term traceability.

---

## 3. Technology Stack

### Backend
- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server

### Frontend
- Razor Views
- Bootstrap
- JavaScript / jQuery where needed
- AJAX for file upload and sync actions where practical

### Storage
- DigitalOcean Spaces or another S3-compatible provider
- Private bucket / private objects only
- Public files are not allowed by default

### Hosting
- Linux VPS / cloud server
- Nginx reverse proxy
- HTTPS using SSL certificate
- Systemd service for .NET app

---

## 4. Architecture Rules

The solution must follow a clean layered architecture.

### Suggested Layers

```txt
SimpleCasePortal.Web
SimpleCasePortal.Application
SimpleCasePortal.Domain
SimpleCasePortal.Infrastructure
```

### Layer Responsibilities

#### Web
- MVC controllers
- Razor views
- ViewModels
- Authentication UI
- Request validation
- User interface only

#### Application
- Services
- DTOs
- Business logic
- API response wrappers
- Use cases
- Validation rules

#### Domain
- Entities
- Enums
- Core business models
- Domain constants

#### Infrastructure
- EF Core DbContext
- Repositories
- File storage implementation
- External API clients
- AppSettings access

---

## 5. Main Modules

### Case Management
- Create case
- Edit case
- View case details
- Search/filter cases
- Soft delete case
- Automatic case numbering

### File Management
- Upload STL files
- Upload photos or other allowed files
- View files
- Download files through secure signed URL
- Soft delete files
- Store file metadata

### User Management
- Doctor users
- Clinic users
- Admin users
- Role assignment
- Permission assignment

### API Sync
- Manual sync to Dentist App
- Manual sync to Workflow App
- Manual sync to Production App
- Sync history
- Retry failed sync
- Log request and response

### Audit
- Login
- Create case
- Update case
- Upload file
- Delete file
- Sync case
- Permission changes

---

## 6. Roles

### SuperAdmin
Full access to everything.

### Admin
Manage cases, files, users, and sync actions.

### Doctor
Create and view own cases, upload files, view own files.

### Clinic
Create and view clinic cases, upload files, view clinic files.

### Viewer
Read-only access.

---

## 7. Permissions

Use granular permissions in addition to roles.

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

---

## 8. Security Memory

Files may need to remain stored for up to 10 years.

Security must be treated as a first-class requirement.

### Rules

- Never save STL files in `wwwroot`.
- Never make bucket public by default.
- Store only object keys in DB, not public URLs.
- Use signed URLs for temporary download/access.
- Use HTTPS only.
- Validate file type and extension.
- Validate file size.
- Sanitize file names.
- Use malware scanning if possible.
- Add audit logs for every important action.
- Apply role and permission checks on every endpoint.
- Use soft delete for cases and files.
- Use retention policies and backups.
- Do not hardcode secrets.
- Use environment variables or secure configuration.
- Encrypt sensitive data where needed.
- Use least privilege access keys for object storage.

---

## 9. Case Numbering

Use database ID as technical PK only.

Use a separate business case number.

Example:

```txt
CP-2026-000001
CP-2026-000002
```

Case number must be generated server-side only.

---

## 10. File Storage Path Convention

Recommended object key:

```txt
cases/{caseNumber}/{fileType}/{yyyy}/{MM}/{uniqueFileId}_{safeFileName}
```

Example:

```txt
cases/CP-2026-000001/UpperSTL/2026/06/9f8a_upper.stl
```

---

## 11. Development Cost Memory

Development cost:

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

## 12. Coding Conventions

- Use async methods where I/O exists.
- Use `ApiResponse<T>` for service responses.
- Do not inject DbContext directly into controllers.
- Controllers call services only.
- Services call repositories / unit of work.
- Use DTOs and ViewModels.
- Keep business logic outside controllers.
- Use soft delete instead of physical delete.
- Add comments only where useful.
- Prefer complete correct solutions over quick fixes.
