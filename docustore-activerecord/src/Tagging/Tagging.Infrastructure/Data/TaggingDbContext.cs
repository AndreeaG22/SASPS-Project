using Microsoft.EntityFrameworkCore;
using Tagging.Domain.Entities;

namespace Tagging.Infrastructure.Data;

public class TaggingDbContext(DbContextOptions<TaggingDbContext> options) : DbContext(options)
{
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<DocumentTag> DocumentTags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("tagging");

        // Configure Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(200);

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            // Create unique index on lowercase name for case-insensitive uniqueness
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Tags_Name_Unique");

            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure DocumentTag entity
        modelBuilder.Entity<DocumentTag>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DocumentId)
                .IsRequired();

            entity.Property(e => e.TagId)
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            // Relationship with Tag
            entity.HasOne(e => e.Tag)
                .WithMany()
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create unique index on DocumentId + TagId to prevent duplicates
            entity.HasIndex(e => new { e.DocumentId, e.TagId })
                .IsUnique()
                .HasDatabaseName("IX_DocumentTags_DocumentId_TagId_Unique");

            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.TagId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "tagging"));
    }
}