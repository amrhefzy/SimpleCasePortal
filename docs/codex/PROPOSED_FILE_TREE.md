# Proposed Solution File Tree

```txt
SimpleCasePortal.sln

/src
  /SimpleCasePortal.Domain
    /Entities
      Case.cs
      CaseFile.cs
      DoctorClinic.cs
      ApplicationUser.cs
      Role.cs
      Permission.cs
      RolePermission.cs
      UserRole.cs
      CaseSyncLog.cs
      AuditLog.cs
      SystemSetting.cs

    /Enums
      CaseStatusEnum.cs
      FileTypeEnum.cs
      SyncTargetEnum.cs
      SyncStatusEnum.cs
      UserTypeEnum.cs

    /Constants
      PermissionNames.cs
      StorageConstants.cs

  /SimpleCasePortal.Application
    /Common
      ApiResponse.cs
      PaginatedResult.cs
      ServiceResult.cs

    /DTOs
      CaseDto.cs
      CreateCaseDto.cs
      UpdateCaseDto.cs
      CaseFileDto.cs
      UploadCaseFileDto.cs
      DoctorClinicDto.cs
      CaseSyncRequestDto.cs
      CaseSyncResultDto.cs

    /Interfaces
      ICaseService.cs
      ICaseFileService.cs
      IUserService.cs
      IRoleService.cs
      IPermissionService.cs
      IFileStorageService.cs
      IExternalSyncService.cs
      IAuditService.cs
      ICaseNumberGenerator.cs
      IUnitOfWork.cs

    /Services
      CaseService.cs
      CaseFileService.cs
      UserService.cs
      RoleService.cs
      PermissionService.cs
      ExternalSyncService.cs
      AuditService.cs
      CaseNumberGenerator.cs

  /SimpleCasePortal.Infrastructure
    /Data
      AppDbContext.cs
      EntityConfigurations
        CaseConfiguration.cs
        CaseFileConfiguration.cs
        DoctorClinicConfiguration.cs
        CaseSyncLogConfiguration.cs
        AuditLogConfiguration.cs

    /Repositories
      GenericRepository.cs
      CaseRepository.cs
      CaseFileRepository.cs
      UserRepository.cs
      UnitOfWork.cs

    /Storage
      DigitalOceanSpacesOptions.cs
      DigitalOceanSpacesStorageService.cs
      SignedUrlService.cs

    /ExternalApis
      ExternalApiOptions.cs
      DentistApiClient.cs
      WorkflowApiClient.cs
      ProductionApiClient.cs

    /Security
      CurrentUserService.cs
      PermissionAuthorizationHandler.cs

  /SimpleCasePortal.Web
    /Controllers
      HomeController.cs
      AccountController.cs
      CasesController.cs
      CaseFilesController.cs
      SyncController.cs
      UsersController.cs
      RolesController.cs
      AuditController.cs

    /ViewModels
      Cases
        CaseListViewModel.cs
        CaseDetailsViewModel.cs
        CreateCaseViewModel.cs
        EditCaseViewModel.cs
      Files
        CaseFileViewModel.cs
        UploadFileViewModel.cs
      Users
        UserListViewModel.cs
        UserFormViewModel.cs

    /Views
      /Cases
        Index.cshtml
        Create.cshtml
        Edit.cshtml
        Details.cshtml
        _CaseFiles.cshtml
        _SyncStatus.cshtml
      /Users
      /Roles
      /Audit
      /Shared

    /wwwroot
      /css
      /js
        cases.js
        file-upload.js
        sync.js

    Program.cs
    appsettings.json
    appsettings.Development.json

/docs
  /codex
    MEMORY_TREE.md
    CODEX_MASTER_PROMPT.md
    IMPLEMENTATION_TASKS.md
```
