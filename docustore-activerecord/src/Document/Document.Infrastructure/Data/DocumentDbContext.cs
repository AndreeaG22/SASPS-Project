using Document.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Document.Infrastructure.Data;

public class DocumentDbContext(DbContextOptions<DocumentDbContext> options) : DbContext(options)
{
    public DbSet<DocumentEntity> Documents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("document");

        // Configure DocumentEntity
        modelBuilder.Entity<DocumentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(2000);
            
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.FilePathOnDisk)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Title);
        });
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "document"));
    }
}