# Security Requirements

## 1. Security Objective

The system will store clinical STL files and patient-related case data. Some files may need to remain available for up to 10 years. Security, traceability, and controlled access are mandatory.

---

## 2. Authentication

- Use secure login.
- Passwords must be hashed using a strong algorithm.
- Enforce HTTPS.
- Use secure cookies.
- Use anti-forgery tokens for forms.
- Optional future enhancement: Multi-factor authentication for admins.

---

## 3. Authorization

- Use role-based access control.
- Add granular permissions for sensitive actions.
- Every controller action must check authorization.
- Doctors and clinics must only access their own cases unless they have elevated permissions.
- Admin-only actions must not be accessible from UI or direct URL.

---

## 4. File Security

### Mandatory Rules

- Do not save uploaded files in `wwwroot`.
- Do not expose permanent public links.
- Use private object storage.
- Store object key in database.
- Generate temporary signed URL for download/view.
- Signed URLs should expire quickly, for example 5 to 30 minutes.
- Validate file extension and MIME type.
- Validate file size.
- Sanitize original file names.
- Use unique stored names.
- Optional: calculate checksum for file integrity.
- Optional: malware scanning pipeline.

### Allowed File Types Initially

```txt
.stl
.jpg
.jpeg
.png
.pdf
.zip
```

STL files are required. Other file types may be allowed based on business need.

---

## 5. Object Storage Security

- Use least privilege access key.
- Storage key should only allow required bucket actions.
- Do not use root cloud keys.
- Bucket must be private.
- Enable versioning if available.
- Enable lifecycle policies where suitable.
- Enable storage logs if possible.
- Use server-side encryption if available.

---

## 6. 10-Year Retention

### Requirements

- Files may be retained for up to 10 years.
- Soft delete must be used in the application database.
- Physical deletion should be restricted to SuperAdmin only.
- Deletion should require audit log.
- Retention metadata should be stored.
- Backups must be tested, not only configured.
- Use storage redundancy.
- Document restoration procedure.

### Recommended Retention Fields

Add later if required:

```txt
RetentionUntil
LegalHold
ArchivedOn
ArchiveStorageClass
```

---

## 7. Audit Logging

Audit logs must be created for:

- Login success/failure
- Case created
- Case updated
- Case soft deleted
- File uploaded
- File downloaded
- File soft deleted
- API sync started
- API sync succeeded
- API sync failed
- User created
- User role changed
- Permission changed

Audit logs should include:

- User ID
- Action
- Entity name
- Entity ID
- Timestamp
- IP address
- User agent
- Old and new values where practical

---

## 8. API Security

Manual API sync must be protected by:

- Permission checks
- Server-to-server API keys or OAuth/client credentials
- HTTPS only
- Request timeout
- Retry policy with limits
- Logging of request and response
- No sensitive secrets in logs
- Idempotency key if supported

---

## 9. Configuration Security

- Do not hardcode connection strings in code.
- Do not hardcode storage access keys.
- Use environment variables, secret manager, or secure server configuration.
- appsettings.Development.json can be used locally but must not contain production secrets in Git.

---

## 10. Backup Strategy

### Database
- Daily automated SQL Server backup.
- Weekly full backup.
- Retain backup history based on business policy.
- Store backups outside the same server.

### Files
- Object storage redundancy.
- Versioning or replication if available.
- Periodic restore test.

---

## 11. Deployment Security

- Run app behind Nginx.
- Enable HTTPS.
- Disable unnecessary ports.
- Use firewall.
- Keep OS updated.
- Use non-root app service user if possible.
- Log application errors securely.
- Monitor disk, CPU, memory, and failed requests.
