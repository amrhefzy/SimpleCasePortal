using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data.EntityConfigurations;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Key).HasMaxLength(150).IsRequired();
        builder.Property(setting => setting.Description).HasMaxLength(500);
        builder.Property(setting => setting.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(setting => setting.Key).IsUnique();
    }
}
