using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data.EntityConfigurations;

public sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("Cases");

        builder.HasKey(caseEntity => caseEntity.Id);

        builder.Property(caseEntity => caseEntity.CaseNumber).HasMaxLength(50).IsRequired();
        builder.Property(caseEntity => caseEntity.PatientName).HasMaxLength(250).IsRequired();
        builder.Property(caseEntity => caseEntity.Gender).HasMaxLength(20);
        builder.Property(caseEntity => caseEntity.CreatedByUserId).HasMaxLength(100).IsRequired();
        builder.Property(caseEntity => caseEntity.Status).HasConversion<int>();
        builder.Property(caseEntity => caseEntity.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(caseEntity => caseEntity.CaseNumber).IsUnique();
        builder.HasIndex(caseEntity => caseEntity.DoctorClinicId);
        builder.HasIndex(caseEntity => caseEntity.Status);
        builder.HasIndex(caseEntity => caseEntity.CreatedOn);
        builder.HasIndex(caseEntity => caseEntity.IsDeleted);

        builder.HasOne(caseEntity => caseEntity.DoctorClinic)
            .WithMany(doctorClinic => doctorClinic.Cases)
            .HasForeignKey(caseEntity => caseEntity.DoctorClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
