using MetadataIndexing.Domain.Common;
using MetadataIndexing.Domain.Services;
using MetadataIndexing.Infrastructure.Data;
using MetadataIndexing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MetadataIndexing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMetadataIndexingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<MetadataIndexingDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(MetadataIndexingDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddSingleton<MetadataIndexingDbContextFactory>(sp =>
        {
            return () =>
            {
                var scope = MetadataIndexing.Domain.Common.ServiceLocator.CreateScope();
                return scope.ServiceProvider.GetRequiredService<MetadataIndexingDbContext>();
            };
        });

        // Register search service
        services.AddScoped<IDocumentSearchService, DocumentSearchService>();

        return services;
    }

    public static void InitializeMetadataIndexingDbContextProvider(IServiceProvider serviceProvider)
    {
        var contextFactory = serviceProvider.GetRequiredService<MetadataIndexingDbContextFactory>();
        MetadataIndexingDbContextProvider.Initialize(contextFactory);
    }
}