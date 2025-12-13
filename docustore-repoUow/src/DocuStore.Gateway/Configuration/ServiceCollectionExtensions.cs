using Document.Application;
using Document.Application.Interfaces;
using Document.Infrastructure;
using MetadataIndexing.Application;
using MetadataIndexing.Application.Interfaces;
using MetadataIndexing.Infrastructure;
using Shared.Events;
using Tagging.Application;
using Tagging.Application.Interfaces;
using Tagging.Infrastructure;
using Versioning.Application;
using Versioning.Application.Interfaces;
using Versioning.Infrastructure;

namespace DocuStore.Gateway.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuStoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add module services - Application layer
        services.AddDocumentApplication();
        services.AddDocumentInfrastructure(configuration);

        services.AddMetadataIndexingApplication();
        services.AddMetadataIndexingInfrastructure(configuration);

        services.AddTaggingApplication();
        services.AddTaggingInfrastructure(configuration);

        services.AddVersioningApplication();
        services.AddVersioningInfrastructure(configuration);

        // Register event publisher
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

        return services;
    }

    public static IServiceCollection AddDocuStoreApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "DocuStore API", Version = "v1" });
            
            // Use full type name (including namespace) as schema ID to avoid conflicts
            // between types with the same name in different modules
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });

        return services;
    }

    public static IServiceCollection AddDocuStoreCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        return services;
    }
}
