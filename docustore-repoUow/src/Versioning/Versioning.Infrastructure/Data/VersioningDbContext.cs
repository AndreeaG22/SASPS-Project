using Microsoft.EntityFrameworkCore;
using Versioning.Domain.Entities;

namespace Versioning.Infrastructure.Data;

public class VersioningDbContext : DbContext
{
    public VersioningDbContext(DbContextOptions<VersioningDbContext> options) : base(options)
    {
    }

    public DbSet<VersionEntity> Versions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("versioning");

        // Configure VersionEntity
        modelBuilder.Entity<VersionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DocumentId)
                .IsRequired();

            entity.Property(e => e.VersionNumber)
                .IsRequired();

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FilePathOnDisk)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.IsCurrent)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => new { e.DocumentId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => new { e.DocumentId, e.IsCurrent });
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "versioning"));
    }
}
