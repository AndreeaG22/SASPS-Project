using MetadataIndexing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetadataIndexing.Infrastructure.Data;

public class MetadataIndexingDbContext : DbContext
{
    public MetadataIndexingDbContext(DbContextOptions<MetadataIndexingDbContext> options) : base(options)
    {
    }

    public DbSet<SearchDocumentIndex> SearchDocumentIndexes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("metadata_indexing");

        // Configure SearchDocumentIndex entity
        modelBuilder.Entity<SearchDocumentIndex>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DocumentId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Create unique index on DocumentId
            entity.HasIndex(e => e.DocumentId)
                .IsUnique()
                .HasDatabaseName("IX_SearchDocumentIndexes_DocumentId_Unique");

            // Indexes for search performance
            entity.HasIndex(e => e.Title)
                .HasDatabaseName("IX_SearchDocumentIndexes_Title");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_SearchDocumentIndexes_CreatedAt");

            entity.HasIndex(e => e.CreatedBy)
                .HasDatabaseName("IX_SearchDocumentIndexes_CreatedBy");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_SearchDocumentIndexes_IsDeleted");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "metadata_indexing"));
    }
}
