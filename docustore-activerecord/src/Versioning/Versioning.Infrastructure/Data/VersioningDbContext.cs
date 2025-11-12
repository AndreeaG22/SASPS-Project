using Microsoft.EntityFrameworkCore;

namespace Versioning.Infrastructure.Data;

public class VersioningDbContext(DbContextOptions<VersioningDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("versioning");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "versioning"));
    }
}