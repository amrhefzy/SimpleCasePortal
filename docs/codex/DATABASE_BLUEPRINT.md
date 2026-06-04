# Database Blueprint

## General Rules

- Use SQL Server.
- Use EF Core Code First.
- Use migrations.
- Use soft delete for business data.
- Use UTC datetime where possible.
- Add indexes for frequent queries.
- Store files as metadata only; physical files live in object storage.

---

## Table: DoctorClinics

Represents doctor or clinic accounts.

| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| Name | nvarchar(250) | Doctor or clinic name |
| Email | nvarchar(250) | Unique if used for login |
| Phone | nvarchar(50) | Optional |
| Country | nvarchar(100) | Optional |
| City | nvarchar(100) | Optional |
| Address | nvarchar(500) | Optional |
| UserType | int | Doctor / Clinic |
| IsActive | bit | Active or disabled |
| CreatedOn | datetime2 | UTC |
| UpdatedOn | datetime2 | Nullable |
| IsDeleted | bit | Soft delete |

---

## Table: Cases

| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| CaseNumber | nvarchar(50) | Unique business reference |
| DoctorClinicId | int | FK |
| PatientName | nvarchar(250) | Required |
| Age | int | Optional |
| DateOfBirth | datetime2 | Optional |
| Gender | nvarchar(20) | Optional |
| Notes | nvarchar(max) | Optional |
| Status | int | Draft / Submitted / Synced / Failed / Archived |
| CreatedByUserId | int/string | Based on user system |
| CreatedOn | datetime2 | UTC |
| UpdatedOn | datetime2 | Nullable |
| IsDeleted | bit | Soft delete |

### Indexes

- Unique index on CaseNumber.
- Index on DoctorClinicId.
- Index on Status.
- Index on CreatedOn.
- Index on IsDeleted.

---

## Table: CaseFiles

| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| CaseId | int | FK |
| FileType | int | UpperSTL / LowerSTL / BiteSTL / Photo / Other |
| OriginalFileName | nvarchar(500) | Original uploaded filename |
| StoredFileName | nvarchar(500) | Safe stored filename |
| ObjectKey | nvarchar(1000) | Storage object key |
| ContentType | nvarchar(150) | MIME type |
| FileExtension | nvarchar(20) | .stl, .jpg, etc. |
| FileSizeBytes | bigint | Size |
| Checksum | nvarchar(128) | Optional |
| UploadedByUserId | int/string | Uploader |
| UploadedOn | datetime2 | UTC |
| IsDeleted | bit | Soft delete |
| DeletedOn | datetime2 | Nullable |
| DeletedByUserId | int/string | Nullable |

### Rules

- Do not store public URL.
- Store object key only.
- Generate signed URL when user requests download/view.

---

## Table: CaseSyncLogs

| Column | Type | Notes |
|---|---|---|
| Id | int | PK |
| CaseId | int | FK |
| SyncTarget | int | Dentist / Workflow / Production |
| SyncStatus | int | Pending / Success / Failed |
| RequestPayload | nvarchar(max) | JSON |
| ResponsePayload | nvarchar(max) | JSON |
| ErrorMessage | nvarchar(max) | Nullable |
| ExternalReferenceId | nvarchar(250) | Optional |
| SyncedByUserId | int/string | User |
| SyncedOn | datetime2 | UTC |
| RetryCount | int | Default 0 |

---

## Table: AuditLogs

| Column | Type | Notes |
|---|---|---|
| Id | bigint | PK |
| UserId | int/string | Nullable |
| Action | nvarchar(150) | Example: Case.Created |
| EntityName | nvarchar(150) | Case, CaseFile, User |
| EntityId | nvarchar(100) | Affected entity |
| OldValues | nvarchar(max) | JSON optional |
| NewValues | nvarchar(max) | JSON optional |
| IpAddress | nvarchar(100) | Optional |
| UserAgent | nvarchar(500) | Optional |
| CreatedOn | datetime2 | UTC |

---

## Table: Roles

| Column | Type |
|---|---|
| Id | int |
| Name | nvarchar(150) |
| Description | nvarchar(500) |
| IsSystemRole | bit |

---

## Table: Permissions

| Column | Type |
|---|---|
| Id | int |
| Name | nvarchar(150) |
| Description | nvarchar(500) |

---

## Table: RolePermissions

| Column | Type |
|---|---|
| RoleId | int |
| PermissionId | int |

---

## Suggested Enums

### CaseStatusEnum

```csharp
public enum CaseStatusEnum
{
    Draft = 1,
    Submitted = 2,
    SyncedToDentist = 3,
    SyncedToWorkflow = 4,
    SyncedToProduction = 5,
    SyncFailed = 6,
    Archived = 7
}
```

### FileTypeEnum

```csharp
public enum FileTypeEnum
{
    UpperSTL = 1,
    LowerSTL = 2,
    BiteSTL = 3,
    Photo = 4,
    Attachment = 5,
    Other = 99
}
```

### SyncTargetEnum

```csharp
public enum SyncTargetEnum
{
    DentistApp = 1,
    WorkflowApp = 2,
    ProductionApp = 3
}
```

### SyncStatusEnum

```csharp
public enum SyncStatusEnum
{
    Pending = 1,
    Success = 2,
    Failed = 3
}
```
