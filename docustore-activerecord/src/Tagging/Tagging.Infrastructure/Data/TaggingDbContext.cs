using Microsoft.EntityFrameworkCore;

namespace Tagging.Infrastructure.Data;

public class TaggingDbContext(DbContextOptions<TaggingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("tagging");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", "tagging"));
    }
}