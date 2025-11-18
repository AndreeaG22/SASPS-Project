using Document.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Versioning.Infrastructure.Data;

namespace DocuStore.Gateway.Configuration;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateDocuStoreDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting Document database migration...");
            var documentContext = services.GetRequiredService<DocumentDbContext>();
            await documentContext.Database.MigrateAsync();
            logger.LogInformation("✅ Document database migration completed successfully!");
            
            logger.LogInformation("Starting Versioning database migration...");
            var versioningContext = services.GetRequiredService<VersioningDbContext>();
            await versioningContext.Database.MigrateAsync();
            logger.LogInformation("✅ Versioning database migration completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}