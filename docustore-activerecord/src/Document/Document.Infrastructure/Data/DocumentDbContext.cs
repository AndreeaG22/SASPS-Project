using Microsoft.EntityFrameworkCore;

namespace Document.Infrastructure.Data;

public class DocumentDbContext(DbContextOptions<DocumentDbContext> options) : DbContext(options)
{
    // Add your DbSets here when you create entities
    // public DbSet<DocumentEntity> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set default schema for this module
        modelBuilder.HasDefaultSchema("document");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Configure migrations history table to use the document schema
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "document"));
    }
}