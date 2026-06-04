# Cost Estimate

## Development Cost

The estimated development cost for the first version of the Simple Case Portal is:

```txt
Total development cost: $2,800
Hourly rate: $40/hour
Estimated total hours: 70 hours
```

## Suggested Development Hours Breakdown

| Work Package | Estimated Hours |
|---|---:|
| Requirement analysis and system design | 6 |
| Solution architecture and project setup | 6 |
| Database design and EF Core setup | 8 |
| User, roles, and permissions | 8 |
| Case management module | 10 |
| DigitalOcean / S3 file upload module | 10 |
| Manual API sync module | 8 |
| Security, audit logs, and retention rules | 6 |
| UI/UX pages and dashboard | 5 |
| Testing, fixes, and deployment preparation | 3 |
| **Total** | **70** |

## Running Cost

| Item | Cost |
|---|---:|
| Server deployment / hosting | €600 per year |
| AWS-compatible storage | $600 per year |
| Storage capacity | 1 TB |
| Traffic included | 5 TB |

## Notes

- Storage cost is estimated for 1 TB storage and 5 TB traffic.
- Running cost may increase if storage grows beyond 1 TB.
- Backup, monitoring, and CDN costs may be added later depending on the production setup.
- Long-term file retention up to 10 years should be considered in future storage planning.
