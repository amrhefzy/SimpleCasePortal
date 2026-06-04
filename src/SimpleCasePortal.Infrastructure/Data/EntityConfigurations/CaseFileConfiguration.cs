using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleCasePortal.Domain.Entities;

namespace SimpleCasePortal.Infrastructure.Data.EntityConfigurations;

public sealed class CaseFileConfiguration : IEntityTypeConfiguration<CaseFile>
{
    public void Configure(EntityTypeBuilder<CaseFile> builder)
    {
        builder.ToTable("CaseFiles");

        builder.HasKey(file => file.Id);

        builder.Property(file => file.FileType).HasConversion<int>();
        builder.Property(file => file.OriginalFileName).HasMaxLength(500).IsRequired();
        builder.Property(file => file.StoredFileName).HasMaxLength(500).IsRequired();
        builder.Property(file => file.ObjectKey).HasMaxLength(1000).IsRequired();
        builder.Property(file => file.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(file => file.FileExtension).HasMaxLength(20).IsRequired();
        builder.Property(file => file.Checksum).HasMaxLength(128);
        builder.Property(file => file.UploadedByUserId).HasMaxLength(100).IsRequired();
        builder.Property(file => file.DeletedByUserId).HasMaxLength(100);
        builder.Property(file => file.UploadedOn).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(file => file.CaseId);
        builder.HasIndex(file => file.FileType);
        builder.HasIndex(file => file.UploadedOn);
        builder.HasIndex(file => file.IsDeleted);

        builder.HasOne(file => file.Case)
            .WithMany(caseEntity => caseEntity.Files)
            .HasForeignKey(file => file.CaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
