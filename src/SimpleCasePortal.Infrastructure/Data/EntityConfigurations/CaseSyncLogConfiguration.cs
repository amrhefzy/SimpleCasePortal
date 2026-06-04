using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data.EntityConfigurations;

public sealed class CaseSyncLogConfiguration : IEntityTypeConfiguration<CaseSyncLog>
{
    public void Configure(EntityTypeBuilder<CaseSyncLog> builder)
    {
        builder.ToTable("CaseSyncLogs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.SyncTarget).HasConversion<int>();
        builder.Property(log => log.SyncStatus).HasConversion<int>();
        builder.Property(log => log.ExternalReferenceId).HasMaxLength(250);
        builder.Property(log => log.SyncedByUserId).HasMaxLength(100).IsRequired();
        builder.Property(log => log.SyncedOn).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(log => log.CaseId);
        builder.HasIndex(log => log.SyncTarget);
        builder.HasIndex(log => log.SyncStatus);
        builder.HasIndex(log => log.SyncedOn);

        builder.HasOne(log => log.Case)
            .WithMany(caseEntity => caseEntity.SyncLogs)
            .HasForeignKey(log => log.CaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
