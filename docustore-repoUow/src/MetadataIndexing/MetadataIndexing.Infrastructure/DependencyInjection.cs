using MetadataIndexing.Application.Interfaces;
using MetadataIndexing.Domain.Services;
using MetadataIndexing.Infrastructure.Data;
using MetadataIndexing.Infrastructure.EventHandlers;
using MetadataIndexing.Infrastructure.Repositories;
using MetadataIndexing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;

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
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        // Register repositories and Unit of Work
        services.AddScoped<ISearchDocumentIndexRepository, SearchDocumentIndexRepository>();
        services.AddScoped<IMetadataIndexingUnitOfWork, MetadataIndexingUnitOfWork>();

        // Register search service
        services.AddScoped<IDocumentSearchService, DocumentSearchService>();

        // Register event handlers
        services.AddScoped<IEventHandler<DocumentCreatedEvent>, DocumentCreatedEventHandler>();
        services.AddScoped<IEventHandler<DocumentUpdatedEvent>, DocumentUpdatedEventHandler>();
        services.AddScoped<IEventHandler<DocumentDeletedEvent>, DocumentDeletedEventHandler>();

        return services;
    }
}
