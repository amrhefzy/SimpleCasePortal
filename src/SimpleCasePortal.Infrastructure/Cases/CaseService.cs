using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Common;
using SimpleCasePortal.Application.DTOs.Cases;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Entities;
using SimpleCasePortal.Domain.Enums;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Cases;

public sealed class CaseService : ICaseService
{
    private const int MaxCreateAttempts = 3;
    private readonly AppDbContext _dbContext;
    private readonly ICaseAuthorizationService _caseAuthorizationService;
    private readonly ICaseNumberGenerator _caseNumberGenerator;
    private readonly IPermissionService _permissionService;
    private readonly IAuditService _auditService;

    public CaseService(
        AppDbContext dbContext,
        ICaseAuthorizationService caseAuthorizationService,
        ICaseNumberGenerator caseNumberGenerator,
        IPermissionService permissionService,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _caseAuthorizationService = caseAuthorizationService;
        _caseNumberGenerator = caseNumberGenerator;
        _permissionService = permissionService;
        _auditService = auditService;
    }

    public async Task<ApiResponse<IReadOnlyCollection<CaseDto>>> GetCasesAsync(
        CaseListFilterDto filter,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var canViewAll = await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewAll, cancellationToken);
        var canViewOwn = await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewOwn, cancellationToken);

        if (!canViewAll && !canViewOwn)
        {
            return ApiResponse<IReadOnlyCollection<CaseDto>>.Fail("You do not have permission to view cases.");
        }

        var query = _dbContext.Cases
            .AsNoTracking()
            .Include(caseEntity => caseEntity.DoctorClinic)
            .Where(caseEntity => !caseEntity.IsDeleted);

        if (!canViewAll)
        {
            var doctorClinicId = await GetUserDoctorClinicIdAsync(userId, cancellationToken);
            if (doctorClinicId is null)
            {
                return ApiResponse<IReadOnlyCollection<CaseDto>>.Ok([]);
            }

            query = query.Where(caseEntity => caseEntity.DoctorClinicId == doctorClinicId.Value);
        }
        else if (filter.DoctorClinicId.HasValue)
        {
            query = query.Where(caseEntity => caseEntity.DoctorClinicId == filter.DoctorClinicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.CaseNumber))
        {
            var caseNumber = filter.CaseNumber.Trim();
            query = query.Where(caseEntity => caseEntity.CaseNumber.Contains(caseNumber));
        }

        if (!string.IsNullOrWhiteSpace(filter.PatientName))
        {
            var patientName = filter.PatientName.Trim();
            query = query.Where(caseEntity => caseEntity.PatientName.Contains(patientName));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(caseEntity => caseEntity.Status == filter.Status.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(caseEntity => caseEntity.CreatedOn >= filter.CreatedFrom.Value.Date);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(caseEntity => caseEntity.CreatedOn < filter.CreatedTo.Value.Date.AddDays(1));
        }

        var cases = await query
            .OrderByDescending(caseEntity => caseEntity.CreatedOn)
            .Select(caseEntity => ToDto(caseEntity))
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CaseDto>>.Ok(cases);
    }

    public async Task<ApiResponse<CaseDto>> GetCaseByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _caseAuthorizationService.CanAccessCaseAsync(userId, id, cancellationToken))
        {
            return ApiResponse<CaseDto>.Fail("You do not have permission to access this case.");
        }

        var caseDto = await _dbContext.Cases
            .AsNoTracking()
            .Include(caseEntity => caseEntity.DoctorClinic)
            .Where(caseEntity => caseEntity.Id == id && !caseEntity.IsDeleted)
            .Select(caseEntity => ToDto(caseEntity))
            .SingleOrDefaultAsync(cancellationToken);

        return caseDto is null
            ? ApiResponse<CaseDto>.Fail("Case was not found.")
            : ApiResponse<CaseDto>.Ok(caseDto);
    }

    public async Task<ApiResponse<CaseDto>> CreateCaseAsync(CreateCaseDto dto, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateCreateAsync(dto, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ApiResponse<CaseDto>.Fail("Case could not be created.", validationErrors);
        }

        var canViewAll = await _permissionService.HasPermissionAsync(dto.CreatedByUserId, PermissionNames.CasesViewAll, cancellationToken);
        var doctorClinicId = canViewAll ? dto.DoctorClinicId!.Value : (await GetUserDoctorClinicIdAsync(dto.CreatedByUserId, cancellationToken))!.Value;

        for (var attempt = 1; attempt <= MaxCreateAttempts; attempt++)
        {
            var caseEntity = new Case
            {
                CaseNumber = await _caseNumberGenerator.GenerateAsync(cancellationToken),
                DoctorClinicId = doctorClinicId,
                PatientName = dto.PatientName.Trim(),
                Age = dto.Age,
                DateOfBirth = dto.DateOfBirth,
                Gender = NormalizeOptional(dto.Gender),
                Notes = NormalizeOptional(dto.Notes),
                Status = CaseStatusEnum.Draft,
                CreatedByUserId = dto.CreatedByUserId,
                CreatedOn = DateTime.UtcNow
            };

            await _dbContext.Cases.AddAsync(caseEntity, cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _auditService.LogAsync(
                    "Case.Created",
                    "Case",
                    caseEntity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    dto.CreatedByUserId,
                    newValues: JsonSerializer.Serialize(new { caseEntity.CaseNumber, caseEntity.PatientName, caseEntity.DoctorClinicId }),
                    cancellationToken: cancellationToken);

                return await GetCaseByIdAsync(caseEntity.Id, dto.CreatedByUserId, cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueCaseNumberViolation(ex) && attempt < MaxCreateAttempts)
            {
                _dbContext.Entry(caseEntity).State = EntityState.Detached;
            }
        }

        return ApiResponse<CaseDto>.Fail("Case number could not be generated uniquely. Please try again.");
    }

    public async Task<ApiResponse<CaseDto>> UpdateCaseAsync(UpdateCaseDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _permissionService.HasPermissionAsync(dto.UpdatedByUserId, PermissionNames.CasesUpdate, cancellationToken))
        {
            return ApiResponse<CaseDto>.Fail("You do not have permission to update cases.");
        }

        if (!await _caseAuthorizationService.CanAccessCaseAsync(dto.UpdatedByUserId, dto.Id, cancellationToken))
        {
            return ApiResponse<CaseDto>.Fail("You do not have permission to update this case.");
        }

        var caseEntity = await _dbContext.Cases
            .Include(entity => entity.DoctorClinic)
            .SingleOrDefaultAsync(entity => entity.Id == dto.Id && !entity.IsDeleted, cancellationToken);

        if (caseEntity is null)
        {
            return ApiResponse<CaseDto>.Fail("Case was not found.");
        }

        var validationErrors = await ValidateUpdateAsync(dto, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ApiResponse<CaseDto>.Fail("Case could not be updated.", validationErrors);
        }

        var canViewAll = await _permissionService.HasPermissionAsync(dto.UpdatedByUserId, PermissionNames.CasesViewAll, cancellationToken);
        var oldValues = new
        {
            caseEntity.PatientName,
            caseEntity.Age,
            caseEntity.DateOfBirth,
            caseEntity.Gender,
            caseEntity.Notes,
            caseEntity.Status,
            caseEntity.DoctorClinicId
        };

        caseEntity.PatientName = dto.PatientName.Trim();
        caseEntity.Age = dto.Age;
        caseEntity.DateOfBirth = dto.DateOfBirth;
        caseEntity.Gender = NormalizeOptional(dto.Gender);
        caseEntity.Notes = NormalizeOptional(dto.Notes);
        caseEntity.UpdatedOn = DateTime.UtcNow;

        if (canViewAll)
        {
            caseEntity.DoctorClinicId = dto.DoctorClinicId!.Value;
            if (dto.Status.HasValue)
            {
                caseEntity.Status = dto.Status.Value;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(
            "Case.Updated",
            "Case",
            caseEntity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            dto.UpdatedByUserId,
            oldValues: JsonSerializer.Serialize(oldValues),
            newValues: JsonSerializer.Serialize(new
            {
                caseEntity.PatientName,
                caseEntity.Age,
                caseEntity.DateOfBirth,
                caseEntity.Gender,
                caseEntity.Notes,
                caseEntity.Status,
                caseEntity.DoctorClinicId
            }),
            cancellationToken: cancellationToken);

        return await GetCaseByIdAsync(caseEntity.Id, dto.UpdatedByUserId, cancellationToken);
    }

    public async Task<ApiResponse<bool>> SoftDeleteCaseAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        if (!await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesDeleteSoft, cancellationToken))
        {
            return ApiResponse<bool>.Fail("You do not have permission to delete cases.");
        }

        if (!await _caseAuthorizationService.CanAccessCaseAsync(userId, id, cancellationToken))
        {
            return ApiResponse<bool>.Fail("You do not have permission to delete this case.");
        }

        var caseEntity = await _dbContext.Cases.SingleOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted, cancellationToken);
        if (caseEntity is null)
        {
            return ApiResponse<bool>.Fail("Case was not found.");
        }

        caseEntity.IsDeleted = true;
        caseEntity.UpdatedOn = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(
            "Case.SoftDeleted",
            "Case",
            caseEntity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            userId,
            oldValues: JsonSerializer.Serialize(new { caseEntity.CaseNumber, IsDeleted = false }),
            newValues: JsonSerializer.Serialize(new { caseEntity.CaseNumber, IsDeleted = true }),
            cancellationToken: cancellationToken);

        return ApiResponse<bool>.Ok(true, "Case deleted successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<DoctorClinicOptionDto>>> GetDoctorClinicOptionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var canViewAll = await _permissionService.HasPermissionAsync(userId, PermissionNames.CasesViewAll, cancellationToken);

        var query = _dbContext.DoctorClinics
            .AsNoTracking()
            .Where(doctorClinic => !doctorClinic.IsDeleted && doctorClinic.IsActive);

        if (!canViewAll)
        {
            var doctorClinicId = await GetUserDoctorClinicIdAsync(userId, cancellationToken);
            if (doctorClinicId is null)
            {
                return ApiResponse<IReadOnlyCollection<DoctorClinicOptionDto>>.Ok([]);
            }

            query = query.Where(doctorClinic => doctorClinic.Id == doctorClinicId.Value);
        }

        var options = await query
            .OrderBy(doctorClinic => doctorClinic.Name)
            .Select(doctorClinic => new DoctorClinicOptionDto { Id = doctorClinic.Id, Name = doctorClinic.Name })
            .ToArrayAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<DoctorClinicOptionDto>>.Ok(options);
    }

    private async Task<List<string>> ValidateCreateAsync(CreateCaseDto dto, CancellationToken cancellationToken)
    {
        var errors = ValidatePatientFields(dto.PatientName, dto.Age, dto.DateOfBirth, dto.Notes);
        if (!await _permissionService.HasPermissionAsync(dto.CreatedByUserId, PermissionNames.CasesCreate, cancellationToken))
        {
            errors.Add("You do not have permission to create cases.");
            return errors;
        }

        var canViewAll = await _permissionService.HasPermissionAsync(dto.CreatedByUserId, PermissionNames.CasesViewAll, cancellationToken);

        if (canViewAll)
        {
            if (!dto.DoctorClinicId.HasValue)
            {
                errors.Add("Doctor/Clinic is required.");
            }
            else if (!await DoctorClinicExistsAsync(dto.DoctorClinicId.Value, cancellationToken))
            {
                errors.Add("Selected Doctor/Clinic is not valid.");
            }
        }
        else if (await GetUserDoctorClinicIdAsync(dto.CreatedByUserId, cancellationToken) is null)
        {
            errors.Add("Your account is not linked to a Doctor/Clinic.");
        }

        return errors;
    }

    private async Task<List<string>> ValidateUpdateAsync(UpdateCaseDto dto, CancellationToken cancellationToken)
    {
        var errors = ValidatePatientFields(dto.PatientName, dto.Age, dto.DateOfBirth, dto.Notes);
        var canViewAll = await _permissionService.HasPermissionAsync(dto.UpdatedByUserId, PermissionNames.CasesViewAll, cancellationToken);

        if (canViewAll)
        {
            if (!dto.DoctorClinicId.HasValue)
            {
                errors.Add("Doctor/Clinic is required.");
            }
            else if (!await DoctorClinicExistsAsync(dto.DoctorClinicId.Value, cancellationToken))
            {
                errors.Add("Selected Doctor/Clinic is not valid.");
            }
        }

        return errors;
    }

    private static List<string> ValidatePatientFields(string patientName, int? age, DateTime? dateOfBirth, string? notes)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(patientName))
        {
            errors.Add("Patient name is required.");
        }

        if (age is < 0 or > 130)
        {
            errors.Add("Age must be between 0 and 130.");
        }

        if (dateOfBirth.HasValue && dateOfBirth.Value.Date > DateTime.UtcNow.Date)
        {
            errors.Add("Date of birth cannot be in the future.");
        }

        if (notes?.Length > 4000)
        {
            errors.Add("Notes cannot exceed 4000 characters.");
        }

        return errors;
    }

    private async Task<bool> DoctorClinicExistsAsync(int doctorClinicId, CancellationToken cancellationToken)
    {
        return await _dbContext.DoctorClinics.AnyAsync(
            doctorClinic => doctorClinic.Id == doctorClinicId && doctorClinic.IsActive && !doctorClinic.IsDeleted,
            cancellationToken);
    }

    private async Task<int?> GetUserDoctorClinicIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive && !user.IsDeleted)
            .Select(user => user.DoctorClinicId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsUniqueCaseNumberViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
            sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
    }

    private static CaseDto ToDto(Case caseEntity)
    {
        return new CaseDto
        {
            Id = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            DoctorClinicId = caseEntity.DoctorClinicId,
            DoctorClinicName = caseEntity.DoctorClinic.Name,
            PatientName = caseEntity.PatientName,
            Age = caseEntity.Age,
            DateOfBirth = caseEntity.DateOfBirth,
            Gender = caseEntity.Gender,
            Notes = caseEntity.Notes,
            Status = caseEntity.Status,
            CreatedOn = caseEntity.CreatedOn,
            UpdatedOn = caseEntity.UpdatedOn
        };
    }
}
