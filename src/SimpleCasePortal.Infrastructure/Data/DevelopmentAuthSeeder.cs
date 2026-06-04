using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Constants;
using SimpleCasePortal.Domain.Entities;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Infrastructure.Data;

public static class DevelopmentAuthSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, bool isDevelopment, IConfiguration configuration)
    {
        var enabled = !bool.TryParse(configuration["DevelopmentSeed:Enabled"], out var parsedEnabled) || parsedEnabled;
        if (!isDevelopment || !enabled)
        {
            return;
        }

        var password = configuration["DevelopmentSeed:TemporaryPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

        var doctorClinicId = await EnsureDoctorClinicAsync(dbContext, "Development Doctor", "doctor@casebridge.local", UserTypeEnum.Doctor);
        var clinicDoctorClinicId = await EnsureDoctorClinicAsync(dbContext, "Development Clinic", "clinic@casebridge.local", UserTypeEnum.Clinic);

        await EnsureUserAsync(dbContext, passwordHasher, "dev-superadmin", "Super Admin", "superadmin@casebridge.local", null, RoleNames.SuperAdmin, password);
        await EnsureUserAsync(dbContext, passwordHasher, "dev-admin", "Admin User", "admin@casebridge.local", null, RoleNames.Admin, password);
        await EnsureUserAsync(dbContext, passwordHasher, "dev-doctor", "Doctor User", "doctor@casebridge.local", doctorClinicId, RoleNames.Doctor, password);
        await EnsureUserAsync(dbContext, passwordHasher, "dev-clinic", "Clinic User", "clinic@casebridge.local", clinicDoctorClinicId, RoleNames.Clinic, password);
        await EnsureUserAsync(dbContext, passwordHasher, "dev-viewer", "Viewer User", "viewer@casebridge.local", null, RoleNames.Viewer, password);
        await EnsureUserAsync(dbContext, passwordHasher, "dev-inactive", "Inactive User", "inactive@casebridge.local", null, RoleNames.Viewer, password, isActive: false);

        await EnsureSampleCasesAsync(dbContext, doctorClinicId, clinicDoctorClinicId);

        await dbContext.SaveChangesAsync();
    }

    private static async Task<int> EnsureDoctorClinicAsync(AppDbContext dbContext, string name, string email, UserTypeEnum userType)
    {
        var existing = await dbContext.DoctorClinics.SingleOrDefaultAsync(doctorClinic => doctorClinic.Email == email);
        if (existing is not null)
        {
            return existing.Id;
        }

        var doctorClinic = new DoctorClinic
        {
            Name = name,
            Email = email,
            UserType = userType,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        await dbContext.DoctorClinics.AddAsync(doctorClinic);
        await dbContext.SaveChangesAsync();

        return doctorClinic.Id;
    }

    private static async Task EnsureUserAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        string id,
        string fullName,
        string email,
        int? doctorClinicId,
        string roleName,
        string temporaryPassword,
        bool isActive = true)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await dbContext.ApplicationUsers.SingleOrDefaultAsync(applicationUser => applicationUser.Id == id);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = id,
                UserName = normalizedEmail,
                FullName = fullName,
                Email = normalizedEmail,
                PasswordHash = passwordHasher.HashPassword(temporaryPassword),
                DoctorClinicId = doctorClinicId,
                IsActive = isActive,
                CreatedOn = DateTime.UtcNow
            };

            await dbContext.ApplicationUsers.AddAsync(user);
        }
        else
        {
            user.FullName = fullName;
            user.UserName = normalizedEmail;
            user.Email = normalizedEmail;
            user.DoctorClinicId = doctorClinicId;
            user.IsActive = isActive;
            user.IsDeleted = false;
            user.UpdatedOn = DateTime.UtcNow;
        }

        var roleId = await dbContext.Roles
            .Where(role => role.Name == roleName)
            .Select(role => role.Id)
            .SingleAsync();

        var hasRole = await dbContext.UserRoles.AnyAsync(userRole => userRole.UserId == id && userRole.RoleId == roleId);
        if (!hasRole)
        {
            await dbContext.UserRoles.AddAsync(new UserRole { UserId = id, RoleId = roleId });
        }
    }

    private static async Task EnsureSampleCasesAsync(AppDbContext dbContext, int doctorClinicId, int clinicDoctorClinicId)
    {
        var hasDoctorCase = await dbContext.Cases.AnyAsync(caseEntity =>
            caseEntity.DoctorClinicId == doctorClinicId &&
            caseEntity.PatientName == "Development Doctor Patient");

        var hasClinicCase = await dbContext.Cases.AnyAsync(caseEntity =>
            caseEntity.DoctorClinicId == clinicDoctorClinicId &&
            caseEntity.PatientName == "Development Clinic Patient");

        var nextSequence = await GetNextCaseSequenceAsync(dbContext);

        if (!hasDoctorCase)
        {
            await dbContext.Cases.AddAsync(new Case
            {
                CaseNumber = FormatCaseNumber(nextSequence++),
                DoctorClinicId = doctorClinicId,
                PatientName = "Development Doctor Patient",
                Age = 31,
                Gender = "Female",
                Notes = "Development sample case for doctor ownership validation.",
                Status = CaseStatusEnum.Draft,
                CreatedByUserId = "dev-doctor",
                CreatedOn = DateTime.UtcNow
            });
        }

        if (!hasClinicCase)
        {
            await dbContext.Cases.AddAsync(new Case
            {
                CaseNumber = FormatCaseNumber(nextSequence),
                DoctorClinicId = clinicDoctorClinicId,
                PatientName = "Development Clinic Patient",
                Age = 42,
                Gender = "Male",
                Notes = "Development sample case for clinic ownership validation.",
                Status = CaseStatusEnum.Submitted,
                CreatedByUserId = "dev-clinic",
                CreatedOn = DateTime.UtcNow
            });
        }
    }

    private static async Task<int> GetNextCaseSequenceAsync(AppDbContext dbContext)
    {
        var prefix = $"CP-{DateTime.UtcNow.Year}-";
        var caseNumbers = await dbContext.Cases
            .AsNoTracking()
            .Where(caseEntity => caseEntity.CaseNumber.StartsWith(prefix))
            .Select(caseEntity => caseEntity.CaseNumber)
            .ToArrayAsync();

        return caseNumbers
            .Select(caseNumber => int.TryParse(caseNumber[prefix.Length..], out var sequence) ? sequence : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;
    }

    private static string FormatCaseNumber(int sequence)
    {
        return $"CP-{DateTime.UtcNow.Year}-{sequence:000000}";
    }
}
