# API Sync Contracts

## Overview

The application supports manual API sync to external systems.

Targets:

1. Dentist App
2. Workflow App
3. Production App

Sync happens only when an authorized user clicks a sync button.

---

## Sync Rules

- User must have the required permission.
- System loads case and related files.
- System builds target-specific payload.
- System sends request to configured API endpoint.
- System logs request, response, status, error, and timestamp.
- System updates case sync state if successful.
- If failed, system keeps error message and allows retry.

---

## Permissions

```txt
Sync.Dentist
Sync.Workflow
Sync.Production
Sync.Retry
```

---

## General Payload

```json
{
  "caseNumber": "CP-2026-000001",
  "patient": {
    "name": "Patient Name",
    "age": 28,
    "gender": "Male",
    "dateOfBirth": "1998-01-01"
  },
  "doctorClinic": {
    "id": 1,
    "name": "Clinic Name",
    "email": "clinic@example.com"
  },
  "files": [
    {
      "fileType": "UpperSTL",
      "originalFileName": "upper.stl",
      "objectKey": "cases/CP-2026-000001/UpperSTL/2026/06/abc_upper.stl",
      "fileSizeBytes": 1234567,
      "downloadUrl": "TEMP_SIGNED_URL_IF_REQUIRED"
    }
  ],
  "notes": "Case notes",
  "createdOn": "2026-06-03T10:00:00Z"
}
```

---

## Important File URL Rule

Preferred integration design:

- Send object keys and metadata if the receiving system can access the same storage securely.
- Otherwise send short-lived signed URLs.
- Do not send permanent public URLs.

---

## Sync Log Example

```json
{
  "caseId": 15,
  "syncTarget": "WorkflowApp",
  "syncStatus": "Success",
  "requestPayload": "{...}",
  "responsePayload": "{...}",
  "externalReferenceId": "WF-2026-123",
  "syncedByUserId": "5",
  "syncedOn": "2026-06-03T10:15:00Z"
}
```

---

## Response Handling

### Success Response

```json
{
  "success": true,
  "externalReferenceId": "WF-2026-123",
  "message": "Case created successfully"
}
```

### Failure Response

```json
{
  "success": false,
  "errorCode": "VALIDATION_ERROR",
  "message": "Upper STL file is required"
}
```

---

## Retry Policy

- Failed sync can be retried manually.
- Retry count must be tracked.
- Do not infinite retry.
- Every retry creates or updates sync log.
- Display latest sync error in UI.

---

## Suggested UI Buttons

In Case Details screen:

```txt
[Sync to Dentist App]
[Sync to Workflow App]
[Sync to Production App]
```

Each button appears only if user has permission.

Display status:

```txt
Dentist Sync: Not Synced / Success / Failed
Workflow Sync: Not Synced / Success / Failed
Production Sync: Not Synced / Success / Failed
```
