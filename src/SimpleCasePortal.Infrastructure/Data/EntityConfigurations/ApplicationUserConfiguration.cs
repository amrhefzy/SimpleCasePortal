using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data.EntityConfigurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).HasMaxLength(100);
        builder.Property(user => user.UserName).HasMaxLength(150).IsRequired();
        builder.Property(user => user.FullName).HasMaxLength(250).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(250).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(1000).IsRequired();
        builder.Property(user => user.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(user => user.UserName).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.DoctorClinicId);
        builder.HasIndex(user => user.IsDeleted);

        builder.HasOne(user => user.DoctorClinic)
            .WithMany(doctorClinic => doctorClinic.Users)
            .HasForeignKey(user => user.DoctorClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
