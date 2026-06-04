using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Case> Cases => Set<Case>();

    public DbSet<CaseFile> CaseFiles => Set<CaseFile>();

    public DbSet<CaseSyncLog> CaseSyncLogs => Set<CaseSyncLog>();

    public DbSet<DoctorClinic> DoctorClinics => Set<DoctorClinic>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
