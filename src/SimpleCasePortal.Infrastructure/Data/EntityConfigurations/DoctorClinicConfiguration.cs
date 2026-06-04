using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data.EntityConfigurations;

public sealed class DoctorClinicConfiguration : IEntityTypeConfiguration<DoctorClinic>
{
    public void Configure(EntityTypeBuilder<DoctorClinic> builder)
    {
        builder.ToTable("DoctorClinics");

        builder.HasKey(doctorClinic => doctorClinic.Id);

        builder.Property(doctorClinic => doctorClinic.Name).HasMaxLength(250).IsRequired();
        builder.Property(doctorClinic => doctorClinic.Email).HasMaxLength(250);
        builder.Property(doctorClinic => doctorClinic.Phone).HasMaxLength(50);
        builder.Property(doctorClinic => doctorClinic.Country).HasMaxLength(100);
        builder.Property(doctorClinic => doctorClinic.City).HasMaxLength(100);
        builder.Property(doctorClinic => doctorClinic.Address).HasMaxLength(500);
        builder.Property(doctorClinic => doctorClinic.UserType).HasConversion<int>();
        builder.Property(doctorClinic => doctorClinic.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(doctorClinic => doctorClinic.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.HasIndex(doctorClinic => doctorClinic.UserType);
        builder.HasIndex(doctorClinic => doctorClinic.IsActive);
        builder.HasIndex(doctorClinic => doctorClinic.IsDeleted);
    }
}
