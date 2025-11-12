using Microsoft.EntityFrameworkCore;

namespace MetadataIndexing.Infrastructure.Data;

public class MetadataIndexingDbContext(DbContextOptions<MetadataIndexingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("metadata_indexing");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "metadata_indexing"));
    }
}