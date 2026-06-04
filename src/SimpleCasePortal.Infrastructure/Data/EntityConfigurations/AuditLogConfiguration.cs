using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data.EntityConfigurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.UserId).HasMaxLength(100);
        builder.Property(log => log.Action).HasMaxLength(150).IsRequired();
        builder.Property(log => log.EntityName).HasMaxLength(150).IsRequired();
        builder.Property(log => log.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(log => log.IpAddress).HasMaxLength(100);
        builder.Property(log => log.UserAgent).HasMaxLength(500);
        builder.Property(log => log.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(log => log.UserId);
        builder.HasIndex(log => log.Action);
        builder.HasIndex(log => log.EntityName);
        builder.HasIndex(log => log.CreatedOn);
    }
}
